using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.DependencyInjection;

namespace SimpleAuth.Services
{
    public class ServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEncryptionService, DefaultEncryptionService>();
            serviceCollection.AddTransient<IRoleService, DefaultRoleService>();
            serviceCollection.AddTransient<IPermissionGroupService, DefaultPermissionGroupService>();
            serviceCollection.AddTransient<IUserService, DefaultUserService>();
            serviceCollection.AddTransient<ITokenInfoService, DefaultTokenInfoService>();
        }
    }
}