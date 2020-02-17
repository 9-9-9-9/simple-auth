using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

namespace SimpleAuth.Shared.Validation
{
    public class ValidationModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IRoleValidationService, DefaultRoleValidationService>();
            serviceCollection.AddSingleton<IPermissionGroupValidationService, DefaultPermissionGroupValidationService>();
            serviceCollection.AddSingleton<IUserValidationService, DefaultUserValidationService>();
            serviceCollection.AddSingleton<IRolePartsValidationService, DefaultRolePartsValidationService>();
        }
    }
}