using ConsoleApps.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

namespace AdministratorConsole.Commands
{
    public class AdministratorCommandModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterCommand<GenerateCorpPermissionTokenCommand>();
            serviceCollection.RegisterCommand<GenerateAppPermissionTokenCommand>();
            serviceCollection.RegisterCommand<EncryptCommand>();
            serviceCollection.RegisterCommand<DecryptCommand>();
        }
    }
}
