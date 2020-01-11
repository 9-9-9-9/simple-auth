using ConsoleApps.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

namespace AppManagementConsole.Commands
{
    public class AppManagementCommandModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterCommand<AddRoleCommand>();
            serviceCollection.RegisterCommand<AddRoleGroupCommand>();
            serviceCollection.RegisterCommand<ListingRolesOfGroupCommand>();
            serviceCollection.RegisterCommand<AddRoleToGroupCommand>();
        }
    }
}