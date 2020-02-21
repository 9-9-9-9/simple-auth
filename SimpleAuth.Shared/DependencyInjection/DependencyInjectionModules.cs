using Microsoft.Extensions.DependencyInjection;

namespace SimpleAuth.Shared.DependencyInjection
{
    public class DependencyInjectionModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IServiceResolver, ServiceResolverUsingServiceProvider>();
        }
    }
}