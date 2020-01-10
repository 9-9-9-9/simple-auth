using System;
using System.Threading.Tasks;
using AdministratorConsole.Commands;
using ConsoleApps.Shared;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;

namespace AdministratorConsole
{
    internal static class Program
    {
        internal static async Task Main()
        {
            try
            {
                await new AdminApp().RunAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    internal class AdminApp : AbstractAppRunnable
    {
        protected override IServiceCollection RegisterServiceCollections(IServiceCollection serviceCollection)
        {
            return serviceCollection.RegisterModules<AdministratorCommandModules>();
        }

        protected override async Task DoWorkAsync(IServiceProvider serviceProvider)
        {

        }
    }
}
