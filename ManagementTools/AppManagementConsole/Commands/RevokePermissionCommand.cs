using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    public class RevokePermissionCommand : AbstractCommand
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;
        private readonly IRoleGroupManagementService _roleGroupManagementService;

        public RevokePermissionCommand(IRoleGroupManagementService roleGroupManagementService,
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _roleGroupManagementService = roleGroupManagementService;
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        protected override Task DoMainJob(string[] args)
        {
            var roleGroupName = args[0];
            var roleIdWithoutCorpAndApp = args[1];
            var strPermission = args[2];

            if (!Enum.TryParse(typeof(Permission), strPermission, true, out var enumPermission))
                throw new ArgumentException($"{strPermission} is not a valid permission");

            return _roleGroupManagementService.DeleteRolesAsync(roleGroupName, new DeleteRolesModel
            {
                Roles = new[]
                {
                    new RoleModel
                    {
                        Role =
                            $"{_simpleAuthConfigurationProvider.Corp}{Constants.SplitterRoleParts}{_simpleAuthConfigurationProvider.App}{Constants.SplitterRoleParts}{roleIdWithoutCorpAndApp}",
                        Permission = ((Permission) enumPermission).Serialize()
                    }
                }
            });
        }

        private static readonly string ValidPermissionInput = string.Join(',', Enum.GetValues(typeof(Permission))
            .Cast<Permission>()
            .Select(x => x.ToString()));

        public override string[] GetParametersName()
        {
            return new[]
            {
                "Role Group name", "Role Id without Corp and App parts",
                $"Permission (accepted values: {ValidPermissionInput}"
            };
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1, 2};
    }
}