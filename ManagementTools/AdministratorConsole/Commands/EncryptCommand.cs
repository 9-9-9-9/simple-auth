using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;

namespace AdministratorConsole.Commands
{
    public class EncryptCommand : AbstractCommand
    {
        private readonly IAdministrationService _administrationService;

        public EncryptCommand(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] { 0 };

        protected override Task DoMainJob(string[] args)
        {
            return Print(_administrationService.EncryptUsingMasterEncryptionKey(args[0]));
        }

        public override string[] GetParametersName()
        {
            return new[] {"Text to be encrypted"};
        }
    }
}
