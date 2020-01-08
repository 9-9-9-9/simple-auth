using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleAuth.Client.Services;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public interface ITenantProvider
    {
        Task<string> GetTenantAsync(HttpContext httpContext);
    }

    public class ConfiguredTenantProvider : ITenantProvider
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;

        public ConfiguredTenantProvider(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        public Task<string> GetTenantAsync(HttpContext httpContext)
        {
            return Task.FromResult(_simpleAuthConfigurationProvider.Tenant);
        }
    }
}