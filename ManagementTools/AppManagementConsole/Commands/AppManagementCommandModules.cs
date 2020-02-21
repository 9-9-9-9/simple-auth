using ConsoleApps.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.DependencyInjection;

namespace AppManagementConsole.Commands
{
    public class AppManagementCommandModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterCommand<CreateUserCommand>();
            serviceCollection.RegisterCommand<GetUserCommand>();
            serviceCollection.RegisterCommand<AddRoleCommand>();
            serviceCollection.RegisterCommand<AddPermissionGroupCommand>();
            serviceCollection.RegisterCommand<LockPermissionGroupCommand>();
            serviceCollection.RegisterCommand<ListingPermissionsOfGroupCommand>();
            serviceCollection.RegisterCommand<AddPermissionToGroupCommand>();
            serviceCollection.RegisterCommand<RevokePermissionCommand>();
            serviceCollection.RegisterCommand<RevokeAllPermissionCommand>();
            serviceCollection.RegisterCommand<AssignUserToPermissionGroupCommand>();
        }
    }
}