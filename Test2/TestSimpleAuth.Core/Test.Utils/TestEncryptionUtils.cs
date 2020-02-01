using System.Security.Cryptography;
using NUnit.Framework;
using SimpleAuth.Core.Utils;

namespace Test.SimpleAuth.Core.Utils
{
    public class TestEncryptionUtils
    {
        [Test]
        public void GenerateNewRsaKeyPair()
        {
            var rsaKeyPair = EncryptionUtils.GenerateNewRsaKeyPair(2048);
            Assert.NotNull(rsaKeyPair?.PublicKey);
            Assert.NotNull(rsaKeyPair.PrivateKey);
        }

        [Test]
        public void Serialize_Deserialize()
        {
            var csp = new RSACryptoServiceProvider(2048);
            var privateKey = csp.ExportParameters(true);
            var expected = EncryptionUtils.SerializeKey(privateKey);
            Assert.AreEqual(expected, EncryptionUtils.SerializeKey(EncryptionUtils.DeserializeKey(expected)));
        }
    }
}