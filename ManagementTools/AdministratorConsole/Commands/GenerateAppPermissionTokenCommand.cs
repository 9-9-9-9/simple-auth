using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;

namespace AdministratorConsole.Commands
{
    public class GenerateAppPermissionTokenCommand : AbstractCommand, ICommand
    {
        private readonly IAdministrationService _administrationService;

        public GenerateAppPermissionTokenCommand(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1};

        protected override Task DoMainJob(string[] args)
        {
            return Print(_administrationService.GenerateAppPermissionTokenAsync(args[0], args[1]));
        }

        public override string[] GetParametersName()
        {
            return new[] {"Corp", "App"};
        }
    }
}