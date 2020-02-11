using Microsoft.Extensions.DependencyInjection;

namespace SimpleAuth.Core.DependencyInjection
{
    public class DependencyInjectionModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IServiceResolver, ServiceResolverUsingServiceProvider>();
        }
    }
}