using NUnit.Framework;
using SimpleAuth.Shared.Validation;
using Test.Shared;

namespace Test.SimpleAuth.Shared.Test.Validation
{
    public class TestIUserValidationService : BaseTestClass
    {
        [TestCase("abcdefjhijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?     a", ExpectedResult = true)]
        [TestCase("Secur3dP4$$", ExpectedResult = true)]
        [TestCase(" Secur3dP4$$", ExpectedResult = false)]
        [TestCase("Secur3dP4$$ ", ExpectedResult = false)]
        [TestCase("    Secur3dP4$$", ExpectedResult = false)]
        [TestCase("Secur3dP4$$    ", ExpectedResult = false)]
        [TestCase("Secur3d\tP4$$", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = true)]
        [TestCase(" ", ExpectedResult = false)]
        [TestCase("    ", ExpectedResult = false)]
        [TestCase("aaaaaaaaa", ExpectedResult = false)]
        [TestCase("aaaaaaaaaa", ExpectedResult = false)]
        [TestCase("AAAAAAAAAA", ExpectedResult = false)]
        [TestCase("AAAAAAAAAa", ExpectedResult = false)]
        [TestCase("AAAAAAAA9a", ExpectedResult = false)]
        public bool IsValidPassword(string password)
        {
            return Svc<IUserValidationService>().IsValidPassword(password).IsValid;
        }
        
        [TestCase("a", ExpectedResult = false)]
        [TestCase("AAAAAAAAAAAAAAAAAAAAAAA", ExpectedResult = false)]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaa", ExpectedResult = true)]
        [TestCase("abcdefjhijklmnopqrstuvwxyz1234567890-_.+@", ExpectedResult = true)]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaa ", ExpectedResult = false)]
        [TestCase(" aaaaaaaaaaaaaaaaaaaaaaa", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        [TestCase(" ", ExpectedResult = false)]
        [TestCase("    ", ExpectedResult = false)]
        public bool IsValidUserId(string userId)
        {
            return Svc<IUserValidationService>().IsValidUserId(userId).IsValid;
        }

        [TestCase(null, ExpectedResult = true)]
        [TestCase("example@gmail.com", ExpectedResult = true)]
        [TestCase(" example@gmail.com", ExpectedResult = false)]
        [TestCase("    example@gmail.com", ExpectedResult = false)]
        [TestCase("example @gmail.com", ExpectedResult = false)]
        [TestCase("example    @gmail.com", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase("!!!!!!!!!!!!!!@!!!!!!", ExpectedResult = false)]
        [TestCase("!!!!!!!!!!!!!!", ExpectedResult = false)]
        public bool IsValidEmailAddress(string email)
        {
            return Svc<IUserValidationService>().IsValidEmail(email).IsValid;
        }
    }
}