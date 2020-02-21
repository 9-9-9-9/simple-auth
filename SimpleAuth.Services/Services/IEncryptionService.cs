using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IEncryptionService
    {
        string Encrypt(string plainTextData);
        string Decrypt(string encryptedData);
        bool TryEncrypt(string plainTextData, out string encryptedData);
        bool TryDecrypt(string encryptedData, out string decryptedData);
    }

    public class DefaultEncryptionService : IEncryptionService
    {
        private readonly RSAParameters _publicKey;
        private readonly RSAParameters _privateKey;
        private readonly ILogger<DefaultEncryptionService> _logger;

        public DefaultEncryptionService(EncryptionUtils.EncryptionKeyPair encryptionKeyPair,
            ILogger<DefaultEncryptionService> logger)
        {
            _logger = logger;
            _publicKey = EncryptionUtils.DeserializeKey(encryptionKeyPair.PublicKey);
            _privateKey = EncryptionUtils.DeserializeKey(encryptionKeyPair.PrivateKey);
        }

        public string Encrypt(string plainTextData)
        {
            try
            {
                _logger.LogWarning("Received an encryption request");
                var csp = new RSACryptoServiceProvider();
                csp.ImportParameters(_publicKey);
                var bytesPlainTextData = Encoding.UTF8.GetBytes(plainTextData);
                var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
                var result = Convert.ToBase64String(bytesCypherText);
                _logger.LogInformation("Encrypted successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encryption failure");
                throw;
            }
        }

        public string Decrypt(string encryptedData)
        {
            try
            {
                _logger.LogWarning("Received an decryption request");
                var csp = new RSACryptoServiceProvider();
                csp.ImportParameters(_privateKey);
                var bytesCypherText = Convert.FromBase64String(encryptedData);
                var bytesPlainTextData = csp.Decrypt(bytesCypherText, false);
                var result = Encoding.UTF8.GetString(bytesPlainTextData);
                _logger.LogInformation("Decrypted successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Decryption failure");
                throw;
            }
        }

        public bool TryEncrypt(string plainTextData, out string encryptedData)
        {
            try
            {
                encryptedData = Encrypt(plainTextData);
                return true;
            }
            catch
            {
                encryptedData = default;
                return false;
            }
        }

        public bool TryDecrypt(string encryptedData, out string decryptedData)
        {
            try
            {
                decryptedData = Decrypt(encryptedData);
                return true;
            }
            catch
            {
                decryptedData = default;
                return false;
            }
        }
    }
}