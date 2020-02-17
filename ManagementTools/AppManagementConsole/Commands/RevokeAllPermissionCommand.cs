using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;

namespace AppManagementConsole.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RevokeAllPermissionCommand : AbstractCommand
    {
        private readonly IPermissionGroupManagementService _permissionGroupManagementService;

        public RevokeAllPermissionCommand(IPermissionGroupManagementService permissionGroupManagementService)
        {
            _permissionGroupManagementService = permissionGroupManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            var permissionGroupName = args[0];

            return _permissionGroupManagementService.RevokeAllPermissionsAsync(permissionGroupName);
        }

        public override string[] GetParametersName()
        {
            return new[] {"Permission Group name"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0};
    }
}