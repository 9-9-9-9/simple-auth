using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Middlewares;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.DependencyInjection;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace Microsoft.AspNetCore.Builder
{
    public static class Extensions
    {
        public static IServiceCollection UseCustomTenantProvider<TTenantProvider>(
            this IServiceCollection serviceCollection)
            where TTenantProvider : class, ITenantProvider
        {
            serviceCollection.AddTransient<ITenantProvider, TTenantProvider>();
            return serviceCollection;
        }

        public static IServiceCollection UseConfiguredTenantProvider(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITenantProvider, ConfiguredTenantProvider>();
            return serviceCollection;
        }

        public static IApplicationBuilder UseSimpleAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SaAuthorizationMiddleware>();
        }

        public static IServiceCollection UseSimpleAuthDefaultServices(this IServiceCollection services,
            IConfiguration simpleAuthSettingsConfiguration = null)
        {
            if (simpleAuthSettingsConfiguration != null)
                services
                    .Configure<SimpleAuthSettings>(simpleAuthSettingsConfiguration);
            
            services
                .RegisterModules<BasicServiceModules>()
                .RegisterModules<ServiceModules>()
                .UseConfiguredTenantProvider();
            return services;
        }
    }
}