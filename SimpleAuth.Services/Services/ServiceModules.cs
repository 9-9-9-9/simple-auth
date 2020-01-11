using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

namespace SimpleAuth.Services
{
    public class ServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEncryptionService, DefaultEncryptionService>();
            serviceCollection.AddTransient<IRoleService, DefaultRoleService>();
            serviceCollection.AddTransient<IRoleGroupService, DefaultRoleGroupService>();
            serviceCollection.AddTransient<IUserService, DefaultUserService>();
            serviceCollection.AddSingleton<ITokenInfoService, DefaultTokenInfoService>();
        }
    }
}