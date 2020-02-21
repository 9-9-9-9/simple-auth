using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.DependencyInjection;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public class ServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IAuthenticationInfoProvider, DefaultAuthenticationInfoProvider>();
            serviceCollection.AddSingleton<IClaimTransformingService, LocalCachingClaimService>();
            serviceCollection.AddSingleton<IJsonService, JsonNetService>();
        }
    }
}