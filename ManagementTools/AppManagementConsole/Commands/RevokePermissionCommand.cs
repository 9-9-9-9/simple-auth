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
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RevokePermissionCommand : AbstractCommand
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;
        private readonly IPermissionGroupManagementService _permissionGroupManagementService;

        public RevokePermissionCommand(IPermissionGroupManagementService permissionGroupManagementService,
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _permissionGroupManagementService = permissionGroupManagementService;
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        protected override Task DoMainJob(string[] args)
        {
            var permissionGroupName = args[0];
            var roleIdWithoutCorpAndApp = args[1];
            var strPermission = args[2];

            if (!Enum.TryParse(typeof(Verb), strPermission, true, out var enumPermission))
                throw new ArgumentException($"{strPermission} is not a valid permission");

            return _permissionGroupManagementService.RevokePermissionsAsync(permissionGroupName, 
                new PermissionModel
                {
                    Role =
                        $"{_simpleAuthConfigurationProvider.Corp}{Constants.SplitterRoleParts}{_simpleAuthConfigurationProvider.App}{Constants.SplitterRoleParts}{roleIdWithoutCorpAndApp}",
                    Verb = ((Verb) enumPermission).Serialize()
                });
        }

        private static readonly string ValidPermissionInput = string.Join(',', Enum.GetValues(typeof(Verb))
            .Cast<Verb>()
            .Select(x => x.ToString()));

        public override string[] GetParametersName()
        {
            return new[]
            {
                "Permission Group name", "Role Id without Corp and App parts",
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