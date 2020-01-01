using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

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