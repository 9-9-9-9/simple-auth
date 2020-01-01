using System;
using System.Security.Cryptography;
using System.Text;
using SimpleAuth.Core.Utils;

namespace SimpleAuth.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainTextData);
        string Decrypt(string encryptedData);
    }

    public class DefaultEncryptionService : IEncryptionService
    {
        private readonly RSAParameters _publicKey;
        private readonly RSAParameters _privateKey;

        public DefaultEncryptionService(EncryptionUtils.EncryptionKeyPair encryptionKeyPair)
        {
            _publicKey = EncryptionUtils.DeserializeKey(encryptionKeyPair.PublicKey);
            _privateKey = EncryptionUtils.DeserializeKey(encryptionKeyPair.PrivateKey);
        }

        public string Encrypt(string plainTextData)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(_publicKey);
            var bytesPlainTextData = Encoding.UTF8.GetBytes(plainTextData);
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
            return Convert.ToBase64String(bytesCypherText);
        }

        public string Decrypt(string encryptedData)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(_privateKey);
            var bytesCypherText = Convert.FromBase64String(encryptedData);
            var bytesPlainTextData = csp.Decrypt(bytesCypherText, false);
            return Encoding.UTF8.GetString(bytesPlainTextData);
        }
    }
}