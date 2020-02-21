using ConsoleApps.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.DependencyInjection;

namespace CorpManagementConsole.Commands
{
    public class CorpManagementCommandModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterCommand<GenerateAppPermissionTokenCommand>();
        }
    }
}