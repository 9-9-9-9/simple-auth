using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Shared.Utils;
using SimpleAuth.Shared;

namespace AdministratorConsole.Commands
{
    public class GenerateSecretKeyPairCommand : AbstractCommand
    {
        protected override Task DoMainJob(string[] args)
        {
            var keyPair = EncryptionUtils.GenerateNewRsaKeyPair(Constants.Encryption.KeySize);
            Print("\nPrivate key:");
            Print(keyPair.PrivateKey);
            Print("\nPublic key:");
            Print(keyPair.PublicKey);
            
            return Task.CompletedTask;
        }

        public override string[] GetParametersName()
        {
            return new string[0];
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new int[0];
    }
}