using System;

namespace SimpleAuth.Core.DependencyInjection
{
    public interface IServiceResolver
    {
        object GetService(Type serviceType);
    }

    public class ServiceResolverUsingServiceProvider : IServiceResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceResolverUsingServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }

    public static class ServiceResolverExtensions
    {
        public static T GetService<T>(this IServiceResolver serviceResolver)
        {
            return (T) serviceResolver.GetService(typeof(T));
        }
        
        public static T GetRequiredService<T>(this IServiceResolver serviceResolver)
        {
            return serviceResolver.GetService<T>();
        }
    }
}