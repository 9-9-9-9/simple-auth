using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleAuth.Core.Utils;
using SimpleAuth.Services;
using SimpleAuth.Shared;
using Test.Shared;

namespace Test.SimpleAuth.Shared.Services
{
    public class TestIEncryptionService : BaseTestClass
    {
        protected override void RegisteredServices(IServiceCollection serviceCollection)
        {
            base.RegisteredServices(serviceCollection);
            serviceCollection.AddSingleton(new EncryptionUtils.EncryptionKeyPair
            {
                PublicKey = Constants.Test.Rsa2048PublicKey,
                PrivateKey = Constants.Test.Rsa2048PrivateKey
            });
            serviceCollection.AddSingleton<IEncryptionService, DefaultEncryptionService>();
        }

        [Test]
        public void DefaultService()
        {
            var serviceProvider = Prepare();
            var svc = serviceProvider.GetRequiredService<IEncryptionService>();
            var originalPlainText = "Hello World";
            var encrypted = svc.Encrypt(originalPlainText);
            var decrypted = svc.Decrypt(encrypted);
            Assert.AreEqual(originalPlainText, decrypted);
        }
    }
}