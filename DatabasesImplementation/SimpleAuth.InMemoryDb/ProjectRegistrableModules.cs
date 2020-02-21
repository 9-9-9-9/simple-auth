using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.DependencyInjection;
using SimpleAuth.InMemoryDb.Services;

namespace SimpleAuth.InMemoryDb
{
    public class ProjectRegistrableModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterModules<InMemoryDbServiceModules>();
        }
    }
}