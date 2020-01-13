using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;

namespace AppManagementConsole.Commands
{
    public class LockRoleGroupCommand : AbstractCommand
    {
        private readonly IRoleGroupManagementService _roleGroupManagementService;

        public LockRoleGroupCommand(IRoleGroupManagementService roleGroupManagementService)
        {
            _roleGroupManagementService = roleGroupManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            var roleGroupName = args[0]; 
            var @lock = args[1].Trim().EqualsIgnoreCase("lock");
            return _roleGroupManagementService.SetLockRoleGroup(roleGroupName, @lock);
        }

        public override string[] GetParametersName()
        {
            return new[] {"Role Group name", "Lock or Unlock"};
        }

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

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1};
    }
}