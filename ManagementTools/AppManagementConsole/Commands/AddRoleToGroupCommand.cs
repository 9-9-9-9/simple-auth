using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    public class AddRoleToGroupCommand : AbstractCommand
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;
        private readonly IRoleGroupManagementService _roleGroupManagementService;

        public AddRoleToGroupCommand(IRoleGroupManagementService roleGroupManagementService,
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _roleGroupManagementService = roleGroupManagementService;
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        protected override async Task DoMainJob(string[] args)
        {
            var validPermissionInput = string.Join(',', Enum.GetValues(typeof(Permission))
                .Cast<Permission>()
                .Select(x => x.ToString()));
            var roleModels = new List<RoleModel>();
            do
            {
                "Role Id without Corp and App parts (leave empty to submit)".Write();
                var roleIdWithoutCorpAndApp = Console.ReadLine();
                if (roleIdWithoutCorpAndApp.IsBlank())
                    break;
                $"Permission (accepted values: {validPermissionInput}".Write();
                var permission = Console.ReadLine();

                if (!Enum.TryParse(typeof(Permission), permission, true, out var enumPermission))
                    throw new ArgumentException($"{permission} is not a valid permission");

                roleModels.Add(new RoleModel
                {
                    Role =
                        $"{_simpleAuthConfigurationProvider.Corp}{Constants.SplitterRoleParts}{_simpleAuthConfigurationProvider.App}{Constants.SplitterRoleParts}{roleIdWithoutCorpAndApp}",
                    Permission = ((Permission) enumPermission).Serialize()
                });
            } while (true);

            if (!roleModels.Any())
            {
                Print("No role added, command skipped");
                return;
            }

            await _roleGroupManagementService.AddRoleToGroupAsync(args[0], new UpdateRolesModel
            {
                Roles = roleModels.ToArray()
            });
            Print("Added");
        }

        public override string[] GetParametersName()
        {
            return new[] {"Role Group name"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0};
    }
}