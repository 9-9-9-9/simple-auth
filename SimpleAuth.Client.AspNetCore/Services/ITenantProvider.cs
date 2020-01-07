using Microsoft.AspNetCore.Http;
using SimpleAuth.Client.Services;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public interface ITenantProvider
    {
        string GetTenant(HttpContext httpContext);
    }

    public class RuntimeTenantProvider : ITenantProvider
    {
        public string GetTenant(HttpContext httpContext)
        {
            // TODO IMPL
            return nameof(RuntimeTenantProvider);
        }
    }

    public class ConfiguredTenantProvider : ITenantProvider
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;

        public ConfiguredTenantProvider(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        public string GetTenant(HttpContext httpContext)
        {
            return _simpleAuthConfigurationProvider.Tenant;
        }
    }
}