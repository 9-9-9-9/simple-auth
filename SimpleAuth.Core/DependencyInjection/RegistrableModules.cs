using Microsoft.Extensions.DependencyInjection;

namespace SimpleAuth.Core.DependencyInjection
{
    public abstract class RegistrableModules
    {
        public abstract void RegisterModules(IServiceCollection serviceCollection);
    }

    public static class RegistrableModulesExtensions
    {
        public static IServiceCollection RegisterModules<T>(this IServiceCollection serviceCollection)
            where T : RegistrableModules, new()
        {
            new T().RegisterModules(serviceCollection);
            return serviceCollection;
        }
    }
}