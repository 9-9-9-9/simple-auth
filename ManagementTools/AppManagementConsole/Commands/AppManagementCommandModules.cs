using ConsoleApps.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

namespace AppManagementConsole.Commands
{
    public class AppManagementCommandModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterCommand<CreateUserCommand>();
            serviceCollection.RegisterCommand<GetUserCommand>();
            serviceCollection.RegisterCommand<AddRoleCommand>();
            serviceCollection.RegisterCommand<AddRoleGroupCommand>();
            serviceCollection.RegisterCommand<LockRoleGroupCommand>();
            serviceCollection.RegisterCommand<ListingRolesOfGroupCommand>();
            serviceCollection.RegisterCommand<AddRoleToGroupCommand>();
            serviceCollection.RegisterCommand<RevokePermissionCommand>();
            serviceCollection.RegisterCommand<RevokeAllPermissionCommand>();
            serviceCollection.RegisterCommand<AssignUserToRoleGroupCommand>();
        }
    }
}