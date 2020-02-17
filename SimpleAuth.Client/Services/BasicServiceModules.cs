using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

namespace SimpleAuth.Client.Services
{
    public class BasicServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IAdministrationService, DefaultAdministrationService>();
            
            serviceCollection.AddSingleton<IRoleManagementService, DefaultRoleManagementService>();
            
            serviceCollection.AddSingleton<IPermissionGroupManagementService, DefaultPermissionGroupManagementService>();
            
            serviceCollection.AddSingleton<IUserManagementService, DefaultUserManagementService>();
            
            serviceCollection.AddSingleton<IUserAuthService, DefaultUserAuthService>();
            
            serviceCollection.AddSingleton<ISimpleAuthConfigurationProvider, DefaultSimpleAuthConfigurationProvider>();
            
            serviceCollection.AddTransient<IHttpService, DefaultHttpService>();
        }
    }
}