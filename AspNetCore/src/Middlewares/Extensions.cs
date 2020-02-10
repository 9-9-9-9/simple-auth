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
            return app
                    .UseMiddleware<SaPushClaimsToContextMiddleware>()
#if !NETCOREAPP2_1
                .UseMiddleware<SaAuthorizationMiddleware>()
#endif
                ;
        }

        public static IApplicationBuilder UseSimpleAuthEndPoints(this IApplicationBuilder app)
        {
            app.Map("/api/simple-auth/roles", builder => { builder.UseMiddleware<SaGetRoles>(); });
            app.Map("/api/simple-auth/check-roles", builder => { builder.UseMiddleware<SaCheckRole>(); });
            return app;
        }

        public static IServiceCollection UseSimpleAuthDefaultServices(this IServiceCollection services,
            IConfigurationSection simpleAuthSettingsConfiguration = null)
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

        public static IServiceCollection UseSimpleAuthDefaultServices(this IServiceCollection services,
            IConfigurationRoot configuration = null)
        {
            services.UseSimpleAuthDefaultServices(configuration?.GetSection(nameof(SimpleAuthSettings)));
            return services;
        }

        public static IServiceCollection StorePermissionInClaim(this IServiceCollection services)
        {
            return services.AddSingleton<IClaimTransformingService, SelfContainedClaimService>();
        }

        public static IServiceCollection StorePermissionInMemoryReferClaimByKey(this IServiceCollection services)
        {
            return services.AddSingleton<IClaimTransformingService, LocalCachingClaimService>();
        }
    }
}