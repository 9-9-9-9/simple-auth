using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Shared.Validation;

namespace SimpleAuth.Shared
{
    public class ProjectRegistrableModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection
                .RegisterModules<ValidationModules>()
                .RegisterModules<DependencyInjectionModules>();
        }
    }
}