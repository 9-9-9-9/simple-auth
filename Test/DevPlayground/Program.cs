using System;
using SimpleAuth.Core.Utils;
using SimpleAuth.Services;
using SimpleAuth.Shared;

namespace DevPlayground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var encrypt = new DefaultEncryptionService(new EncryptionUtils.EncryptionKeyPair
            {
                PublicKey = Constants.Test.Rsa2048PublicKey,
                PrivateKey = Constants.Test.Rsa2048PrivateKey
            }).Encrypt("LoremIpSum");
            Console.WriteLine(encrypt);
        }
    }
}