using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Repositories;

namespace SimpleAuth.Services
{
    public class ProjectRegistrableModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection
                .RegisterModules<RepositoryModules>()
                .RegisterModules<ServiceModules>();
        }
    }
}