using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.DependencyInjection;
using SimpleAuth.Services;

namespace Test.SimpleAuth.Shared.Mock.Services
{
    public class MockServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEncryptionService, DummyEncryptionService>();
        }
    }
}