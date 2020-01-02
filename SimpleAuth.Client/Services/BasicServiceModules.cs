using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

namespace SimpleAuth.Client.Services
{
    public class BasicServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IAuthService, DefaultAuthService>();
            
            serviceCollection.AddSingleton<ISimpleAuthConfigurationProvider, DefaultSimpleAuthConfigurationProvider>();
            
            serviceCollection.AddTransient<IHttpService, DefaultHttpService>();
        }
    }
}