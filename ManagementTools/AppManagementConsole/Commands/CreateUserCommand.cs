using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    public class CreateUserCommand : AbstractCommand
    {
        private readonly IUserManagementService _userManagementService;

        public CreateUserCommand(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            var userId = args[0];
            return _userManagementService.CreateUserAsync(new CreateUserModel
            {
                UserId = userId
            });
        }

        public override string[] GetParametersName()
        {
            return new[] {"User Id"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0};
    }
}