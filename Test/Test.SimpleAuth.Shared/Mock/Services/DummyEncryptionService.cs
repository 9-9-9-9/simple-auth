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
    }
}