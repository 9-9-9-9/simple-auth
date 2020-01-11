using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    public class AssignUserToRoleGroupCommand : AbstractCommand
    {
        private readonly IUserManagementService _userManagementService;

        public AssignUserToRoleGroupCommand(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            var userId = args[0];
            var roleGroup = args[1];

            return _userManagementService.AssignUserToGroupsAsync(userId, new ModifyUserRoleGroupsModel
            {
                RoleGroups = new[] {roleGroup}
            });
        }

        public override string[] GetParametersName()
        {
            return new[] {"User Id", "Role Group name"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1};
    }
}