using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;

namespace AppManagementConsole.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LockPermissionGroupCommand : AbstractCommand
    {
        private readonly IPermissionGroupManagementService _permissionGroupManagementService;

        public LockPermissionGroupCommand(IPermissionGroupManagementService permissionGroupManagementService)
        {
            _permissionGroupManagementService = permissionGroupManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            var permissionGroupName = args[0];
            var @lock = args[1].Trim().EqualsIgnoreCase("lock");
            return _permissionGroupManagementService.SetLockPermissionGroup(permissionGroupName, @lock);
        }

        public override string[] GetParametersName()
        {
            return new[] {"Permission Group name", "Lock or Unlock"};
        }

        // ReSharper disable RedundantJumpStatement
        // ReSharper disable RedundantIfElseBlock
        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            var state = args[1].Trim();
            if (state.EqualsIgnoreCase("lock"))
                yield break;
            else if (state.EqualsIgnoreCase("unlock"))
                yield break;
            else
                yield return "Only Lock/Unlock are considered as valid value";
        }
        // ReSharper restore RedundantIfElseBlock
        // ReSharper restore RedundantJumpStatement

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1};
    }
}