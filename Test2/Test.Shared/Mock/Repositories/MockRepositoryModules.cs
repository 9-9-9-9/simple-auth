using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.DependencyInjection;

namespace Test.SimpleAuth.Shared.Mock.Repositories
{
    public class MockRepositoryModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDummyMemoryCachedRepository, DummyCachedRepositories>();
        }
    }
}