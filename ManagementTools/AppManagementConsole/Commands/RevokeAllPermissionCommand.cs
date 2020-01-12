using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;

namespace AppManagementConsole.Commands
{
    public class RevokeAllPermissionCommand : AbstractCommand
    {
        private readonly IRoleGroupManagementService _roleGroupManagementService;

        public RevokeAllPermissionCommand(IRoleGroupManagementService roleGroupManagementService)
        {
            _roleGroupManagementService = roleGroupManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            var roleGroupName = args[0];

            return _roleGroupManagementService.DeleteAllRolesAsync(roleGroupName);
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