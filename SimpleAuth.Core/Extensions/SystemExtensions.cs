using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace System
{
    public static class SimpleAuthSystemExtensions
    {
        public static ILogger<T> ResolveLogger<T>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ILogger<T>>();
        }
    }
}