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
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AddPermissionToGroupCommand : AbstractCommand
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;
        private readonly IPermissionGroupManagementService _permissionGroupManagementService;

        public AddPermissionToGroupCommand(IPermissionGroupManagementService permissionGroupManagementService,
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _permissionGroupManagementService = permissionGroupManagementService;
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        protected override async Task DoMainJob(string[] args)
        {
            var validPermissionInput = string.Join(',', Enum.GetValues(typeof(Verb))
                .Cast<Verb>()
                .Select(x => x.ToString()));
            var roleModels = new List<PermissionModel>();
            do
            {
                "Role Id without Corp and App parts (leave empty to submit)".Write();
                var roleIdWithoutCorpAndApp = Console.ReadLine();
                if (roleIdWithoutCorpAndApp.IsBlank())
                    break;
                $"Permission (accepted values: {validPermissionInput}".Write();
                var permission = Console.ReadLine();

                if (!Enum.TryParse(typeof(Verb), permission, true, out var enumPermission))
                    throw new ArgumentException($"{permission} is not a valid permission");

                roleModels.Add(new PermissionModel
                {
                    Role =
                        $"{_simpleAuthConfigurationProvider.Corp}{Constants.SplitterRoleParts}{_simpleAuthConfigurationProvider.App}{Constants.SplitterRoleParts}{roleIdWithoutCorpAndApp}",
                    Verb = ((Verb) enumPermission).Serialize()
                });
            } while (true);

            if (!roleModels.Any())
            {
                Print("No role added, command skipped");
                return;
            }

            await _permissionGroupManagementService.AddRoleToGroupAsync(args[0], new PermissionModels
            {
                Permissions = roleModels.ToArray()
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