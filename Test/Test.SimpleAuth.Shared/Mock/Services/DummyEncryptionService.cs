using SimpleAuth.Services;

namespace Test.SimpleAuth.Shared.Mock.Services
{
    public class DummyEncryptionService : IEncryptionService
    {
        public string Encrypt(string plainTextData)
        {
            return plainTextData;
        }

        public string Decrypt(string encryptedData)
        {
            return encryptedData;
        }

        public bool TryEncrypt(string plainTextData, out string encryptedData)
        {
            encryptedData = plainTextData;
            return true;
        }

        public bool TryDecrypt(string encryptedData, out string decryptedData)
        {
            decryptedData = encryptedData;
            return true;
        }
    }
}