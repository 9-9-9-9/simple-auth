using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.DependencyInjection;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public class ServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IJsonService, JsonNetService>();
            serviceCollection.AddSingleton<IAuthenticationInfoProvider, DefaultAuthenticationInfoProvider>();
        }
    }
}