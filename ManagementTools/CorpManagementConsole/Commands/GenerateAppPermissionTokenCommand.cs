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
            return Print(_administrationService.GenerateAppPermissionTokenAsync(args[0], ParseBoolean(args[1]) ?? true));
        }

        public override string[] GetParametersName()
        {
            return new[] {"App", "Public (y/n)"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            var @public = args[1];
            if (IsCorrectBool(@public))
                yield break;
            else
                yield return "Invalid @public, accepted y/n";
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1};
    }
}