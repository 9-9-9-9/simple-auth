using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;

namespace CorpManagementConsole.Commands
{
    public class GenerateAppPermissionTokenCommand : AbstractCommand
    {
        private readonly IAdministrationService _administrationService;

        public GenerateAppPermissionTokenCommand(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        protected override Task DoMainJob(string[] args)
        {
            return Print(_administrationService.GenerateAppPermissionTokenAsync(args[0]));
        }

        public override string[] GetParametersName()
        {
            return new[] {"App"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0};
    }
}