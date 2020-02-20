using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ConsoleApps.Shared.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.Exceptions;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Core.IO;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

// ReSharper disable InconsistentNaming

namespace PermissionSynchronizer
{
    internal static class Program
    {
        private const string SectionTargetApp = "Target";
        private const string SectionUsers = "Users";
        private const string SectionGroups = "Groups";

        // ReSharper disable RedundantDefaultMemberInitializer
        private static string Corp = null;
        private static string App = null;
        private static string AppToken = null;

        private static IServiceProvider ServiceProvider = null;
        // ReSharper restore RedundantDefaultMemberInitializer

        internal static async Task Main(string[] args)
        {
            await Task.CompletedTask;

            if (!args.IsAny())
                throw new ArgumentException("Require file as parameter");

            var inputFile = args.Single(x => !x.IsBlank());
            if (inputFile.IsBlank() || !File.Exists(inputFile))
                throw new ArgumentException("File not found");

            inputFile.Write();

            var rootTml = TmlFile.Parse(File.ReadAllLines(inputFile));

            var targetNode = rootTml.SingleOrDefault(SectionTargetApp);
            if (targetNode == null)
                throw new InvalidOperationException($"Missing section '{SectionTargetApp}'");
            Corp = targetNode.FirstOrDefault("corp").ChildrenNodes.SingleOrDefault()?.Content;
            App = targetNode.FirstOrDefault("app").ChildrenNodes.SingleOrDefault()?.Content;
            if (Corp.IsBlank() || App.IsBlank())
                throw new ArgumentNullException($"{nameof(Corp)}.{nameof(App)}");

            $"Corp: {Corp}, App: {App}".Write();

            var appTokens = FileUtil.ReadTsvToModel(Constants.FileAppToken, new AppTokenConverter()).ToList();
            AppToken = appTokens.FirstOrDefault(x => x.Corp == Corp && x.App == App)?.AppToken;
            if (AppToken.IsBlank())
                throw new NotSupportedException($"{nameof(Corp)}.{nameof(App)} ({nameof(AppTokenModel.AppToken)})");

            var groupsNode = rootTml.SingleOrDefault(SectionGroups);
            var usersNode = rootTml.SingleOrDefault(SectionUsers);

            var groups = ParseGroups(groupsNode).ToArray();
            var users = ParseUsers(usersNode).ToArray();

            var allRoleIdsWoCA = groups.SelectMany(x => x.PermissionModels.Select(y => y.RoleIdWithoutCorpApp))
                .Distinct().ToArray();
            var allGroupNames = groups.Select(x => x.Name).ToArray();
            var allUserIds = users.Select(x => x.UserId).ToArray();

            if (allGroupNames.Distinct().Count() != allGroupNames.Length)
                throw new ArgumentException($"Duplicated definition of groups within section {SectionGroups}");

            if (allUserIds.Distinct().Count() != allUserIds.Length)
                throw new ArgumentException($"Duplicated definition of users within section {SectionUsers}");

            // DI
            var builder = new ConfigurationBuilder();
            builder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("/configmaps/management-tools/appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();
            services
                .Configure<SimpleAuthSettings>(configuration.GetSection(nameof(SimpleAuthSettings)))
                .RegisterModules<BasicServiceModules>();

            ServiceProvider = services.BuildServiceProvider();

            var simpleAuthConfigurationProvider = ServiceProvider.GetService<ISimpleAuthConfigurationProvider>();
            simpleAuthConfigurationProvider.Corp = Corp;
            simpleAuthConfigurationProvider.App = App;
            simpleAuthConfigurationProvider.AppToken = AppToken;
            //

            CreateRoles(allRoleIdsWoCA);
            CreateGroups(allGroupNames);
            CreateUsers(allUserIds);

            await AddPermissionToGroupsAsync(groups);
            await AddUserToGroupsAsync(users);

            "Done".Write();
        }

        #region Assigning

        private static async Task AddUserToGroupsAsync(ICollection<UserModel> userModels)
        {
            nameof(AddUserToGroupsAsync).Write();

            var userManagementService = ServiceProvider.GetService<IUserManagementService>();

            foreach (var userModel in userModels)
            {
                $"Un-assigning user {userModel.UserId} from permission groups".Write();
                await userManagementService.UnAssignUserFromAllGroupsAsync(userModel.UserId);
            }

            foreach (var userModel in userModels)
            {
                try
                {
                    await userManagementService.AssignUserToGroupsAsync(userModel.UserId,
                        new ModifyUserPermissionGroupsModel
                        {
                            PermissionGroups = userModel.Groups
                        });
                }
                catch (SimpleAuthHttpRequestException e)
                {
                    throw new SimpleAuthHttpRequestException(e.HttpStatusCode,
                        $"Assign user {userModel.UserId} to permission groups");
                }
            }
        }

        private static async Task AddPermissionToGroupsAsync(ICollection<GroupModel> groupModels)
        {
            nameof(AddPermissionToGroupsAsync).Write();

            var permissionGroupManagementService = ServiceProvider.GetService<IPermissionGroupManagementService>();

            foreach (var groupModel in groupModels)
            {
                $"Revoke all permissions of group: {groupModel.Name}".Write();
                await permissionGroupManagementService.RevokeAllPermissionsAsync(groupModel.Name);
            }

            foreach (var groupModel in groupModels.Where(x => x.PermissionModels.IsAny()))
            {
                try
                {
                    await permissionGroupManagementService.AddPermissionToGroupAsync(groupModel.Name,
                        new PermissionModels
                        {
                            Permissions = groupModel.PermissionModels.Select(x =>
                                new SimpleAuth.Shared.Models.PermissionModel
                                {
                                    Role = $"{Corp}.{App}.{x.RoleIdWithoutCorpApp}",
                                    Verb = x.Verb.Serialize()
                                }).ToArray()
                        });
                }
                catch (SimpleAuthHttpRequestException e)
                {
                    throw new SimpleAuthHttpRequestException(e.HttpStatusCode,
                        $"Assign permission to group {groupModel.Name}");
                }
            }
        }

        #endregion

        #region Entities creatation

        private static void CreateRoles(ICollection<string> roleIdsWoCA)
        {
            nameof(CreateRoles).Write();
            var cnt = roleIdsWoCA.Count;

            var roleManagementService = ServiceProvider.GetService<IRoleManagementService>();

            var clientPermissionModels = roleIdsWoCA.Select(roleIdWoCA =>
                    $"{Corp}{SimpleAuth.Shared.Constants.SplitterRoleParts}{App}{SimpleAuth.Shared.Constants.SplitterRoleParts}{roleIdWoCA}")
                .Select(
                    x =>
                    {
                        RoleUtils.Parse(x, out var clientPermissionModel);
                        return clientPermissionModel;
                    });

            var result = clientPermissionModels.Select(async (cpm) =>
            {
                try
                {
                    $"{cnt--} role Ids remaining".Write();
                    await roleManagementService.AddRoleAsync(new CreateRoleModel
                    {
                        Corp = cpm.Corp,
                        App = cpm.App,
                        Env = cpm.Env,
                        Tenant = cpm.Tenant,
                        Module = cpm.Module,
                        SubModules = cpm.SubModules
                    });
                    return (true, HttpStatusCode.Created, cpm.ComputeId());
                }
                catch (SimpleAuthHttpRequestException ex)
                {
                    return (ex.HttpStatusCode == HttpStatusCode.Conflict || ex.HttpStatusCode == HttpStatusCode.Found,
                        ex.HttpStatusCode, cpm.ComputeId());
                }
            }).Select(x => x.Result).ToArray();

            ThrowIfAnyFailure(result);
        }

        private static void CreateGroups(ICollection<string> groups)
        {
            nameof(CreateGroups).Write();
            var cnt = groups.Count;

            var permissionGroupManagementService = ServiceProvider.GetService<IPermissionGroupManagementService>();

            var createGroupModels = groups.Select(x => new CreatePermissionGroupModel
            {
                Corp = Corp,
                App = App,
                Name = x,
            });

            var result = createGroupModels.Select(async (cgm) =>
            {
                try
                {
                    $"{cnt--} groups remaining".Write();
                    await permissionGroupManagementService.AddPermissionGroupAsync(cgm);
                    return (true, HttpStatusCode.Created, cgm.Name);
                }
                catch (SimpleAuthHttpRequestException ex)
                {
                    return (ex.HttpStatusCode == HttpStatusCode.Conflict || ex.HttpStatusCode == HttpStatusCode.Found,
                        ex.HttpStatusCode, cgm.Name);
                }
            }).Select(x => x.Result).ToArray();

            ThrowIfAnyFailure(result);
        }

        private static void CreateUsers(ICollection<string> users)
        {
            nameof(CreateUsers).Write();

            var userManagementService = ServiceProvider.GetService<IUserManagementService>();

            var createUserModels = users.Select(x => new CreateUserModel
            {
                UserId = x,
            });

            var result = createUserModels.Select(async (um) =>
            {
                try
                {
                    await userManagementService.CreateUserAsync(um);
                    return (true, HttpStatusCode.Created, um.UserId);
                }
                catch (SimpleAuthHttpRequestException ex)
                {
                    return (ex.HttpStatusCode == HttpStatusCode.Conflict || ex.HttpStatusCode == HttpStatusCode.Found,
                        ex.HttpStatusCode, um.UserId);
                }
            }).Select(x => x.Result).ToArray();

            ThrowIfAnyFailure(result);
        }

        #endregion

        private static void ThrowIfAnyFailure(ICollection<(bool, HttpStatusCode, string)> input)
        {
            if (input.Any(x => !x.Item1))
                throw new AggregateException(input.Where(x => !x.Item1)
                    .Select(x => new SimpleAuthHttpRequestException(x.Item2, x.Item3)));
        }

        #region Model parsers

        private static IEnumerable<UserModel> ParseUsers(TmlFile.TmlNode usersNode)
        {
            if (usersNode == null)
                yield break;

            foreach (var userNode in usersNode.ChildrenNodes)
                yield return new UserModel
                {
                    UserId = userNode.Content,
                    Groups = userNode.ChildrenNodes.Select(x => x.Content).ToArray()
                };
        }

        private static IEnumerable<PermissionModel> ParsePermissions(TmlFile.TmlNode groupNode)
        {
            if (groupNode == null)
                yield break;

            foreach (var permissionNode in groupNode.ChildrenNodes)
            {
                var errMsgPrefix =
                    $"Permission definition problem at group {groupNode.Content}, line content: {permissionNode.Content}";
                var spl = permissionNode.Content.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (spl.Length < 2)
                    throw new ArgumentException(
                        $"{errMsgPrefix}: require format 'roleId<space>verb1<space>verb2 if any'");

                yield return new PermissionModel
                {
                    RoleIdWithoutCorpApp = spl[0],
                    Verb = Verb.None.Grant(spl.Skip(1).Select(x => Enum.Parse<Verb>(x, true)).ToArray())
                };
            }
        }

        private static IEnumerable<GroupModel> ParseGroups(TmlFile.TmlNode groupsNode)
        {
            if (groupsNode == null)
                yield break;

            foreach (var groupNode in groupsNode.ChildrenNodes)
                yield return new GroupModel
                {
                    Name = groupNode.Content,
                    PermissionModels = ParsePermissions(groupNode).ToArray()
                };
        }

        #endregion

        #region Models

        private class UserModel
        {
            public string UserId { get; set; }
            public string[] Groups { get; set; }
        }

        private class GroupModel
        {
            public string Name { get; set; }
            public PermissionModel[] PermissionModels { get; set; }
        }

        private class PermissionModel
        {
            public string RoleIdWithoutCorpApp { get; set; }
            public Verb Verb { get; set; }
        }

        #endregion
    }
}