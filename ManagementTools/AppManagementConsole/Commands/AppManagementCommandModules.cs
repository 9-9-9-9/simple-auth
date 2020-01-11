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
            serviceCollection.RegisterCommand<AddRoleWith_1_SubModule>();
            serviceCollection.RegisterCommand<AddRoleWith_2_SubModules>();
            serviceCollection.RegisterCommand<AddRoleWith_3_SubModules>();
            serviceCollection.RegisterCommand<AddRoleWith_4_SubModules>();
        }
    }
}