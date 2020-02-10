using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleAuth.Client.Services;

namespace SimpleAuth.Client.AspNetCore.Services
{
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public interface ITenantProvider
    {
        string GetTenant(HttpContext httpContext);
        Task<string> GetTenantAsync(HttpContext httpContext);
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

        public Task<string> GetTenantAsync(HttpContext httpContext)
        {
            return Task.FromResult(GetTenant(httpContext));
        }
    }
}