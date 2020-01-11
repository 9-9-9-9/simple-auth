using System;
using System.Threading.Tasks;
using AdministratorConsole.Commands;
using ConsoleApps.Shared;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.DependencyInjection;

namespace AdministratorConsole
{
    internal static class Program
    {
        internal static async Task<int> Main()
        {
            try
            {
                await new AdminApp().RunAsync();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
        }
    }

    internal class AdminApp : AbstractAppRunnable
    {
        protected override IServiceCollection RegisterServiceCollections(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .RegisterModules<AdministratorCommandModules>()
                .RegisterModules<BasicServiceModules>();
        }
    }
}