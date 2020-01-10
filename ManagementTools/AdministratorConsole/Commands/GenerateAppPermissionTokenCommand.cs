using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdministratorConsole.Commands
{
    public class GenerateAppPermissionTokenCommand : RequestCommandWithResponse<string>
    {
        private readonly IAdministrationService _administrationService;

        public GenerateAppPermissionTokenCommand(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        protected override Task<string> DoRequest(params string[] args)
        {
            return _administrationService.GenerateAppPermissionTokenAsync(args[0], args[1]);
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int NumberOfParameters => 2;
        protected override int[] IdxParametersCanNotBeBlank => new[] { 0, 1 };
    }
}
