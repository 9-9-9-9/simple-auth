using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Core.Utils;

namespace AdministratorConsole.Commands
{
    public class EncryptUsingPublicKeyCommand : AbstractCommand
    {
        protected override Task DoMainJob(string[] args)
        {
            var data = args[0];
            var pk = args[1];
            
            var publicKey = EncryptionUtils.DeserializeKey(pk);
            
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);
            var bytesPlainTextData = Encoding.UTF8.GetBytes(data);
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
            
            var encryptedText = Convert.ToBase64String(bytesCypherText);
            Print("Encrypted text:");
            Print(encryptedText);
            
            return Task.CompletedTask;
        }

        public override string[] GetParametersName()
        {
            return new[] {"Data to be encrypted", "Public key"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1};
    }
}