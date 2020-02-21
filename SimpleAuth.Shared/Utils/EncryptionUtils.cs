using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace SimpleAuth.Shared.Utils
{
    public static class EncryptionUtils
    {
        public static EncryptionKeyPair GenerateNewRsaKeyPair(int dwKeySize)
        {
            var csp = new RSACryptoServiceProvider(dwKeySize);
            var privateKey = csp.ExportParameters(true);
            var publicKey = csp.ExportParameters(false);

            return new EncryptionKeyPair
            {
                PublicKey = SerializeKey(publicKey),
                PrivateKey = SerializeKey(privateKey)
            };
        }

        public static string SerializeKey(RSAParameters rsaParam)
        {
            using var sw = new StringWriter();
            var xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, rsaParam);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sw.ToString()));
        }

        public static RSAParameters DeserializeKey(string source)
        {
            using var sr = new StringReader(Encoding.UTF8.GetString(Convert.FromBase64String(source)));
            var xs = new XmlSerializer(typeof(RSAParameters));
            return (RSAParameters) xs.Deserialize(sr);
        }
        
        public class EncryptionKeyPair
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }
    }
}