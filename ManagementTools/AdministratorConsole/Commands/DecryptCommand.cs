using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdministratorConsole.Commands
{
    public class DecryptCommand : AbstractCommand
    {
        private readonly IAdministrationService _administrationService;

        public DecryptCommand(IAdministrationService administrationService)
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
            return Print(_administrationService.DecryptUsingMasterEncryptionKey(args[0]));
        }

        public override string[] GetParametersName()
        {
            return new[] {"Data to be decrypted"};
        }
    }
}
