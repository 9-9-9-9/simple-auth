using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AssignUserToPermissionGroupCommand : AbstractCommand
    {
        private readonly IUserManagementService _userManagementService;

        public AssignUserToPermissionGroupCommand(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            var userId = args[0];
            var permissionGroup = args[1];

            return _userManagementService.AssignUserToGroupsAsync(userId, new ModifyUserPermissionGroupsModel
            {
                PermissionGroups = new[] {permissionGroup}
            });
        }

        public override string[] GetParametersName()
        {
            return new[] {"User Id", "Permission Group name"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1};
    }
}