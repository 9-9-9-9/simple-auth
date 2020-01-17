using System;
using System.Threading.Tasks;
using AppManagementConsole.Commands;
using ConsoleApps.Shared;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.DependencyInjection;

namespace AppManagementConsole
{
    internal static class Program
    {
        internal static async Task<int> Main()
        {
            try
            {
                await new AppManagementApp().RunAsync();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
        }
    }

    internal class AppManagementApp : AbstractAppRunnable
    {
        protected override bool AllowSwitchingDefaultApp => true;

        protected override IServiceCollection RegisterServiceCollections(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .RegisterModules<AppManagementCommandModules>()
                .RegisterModules<BasicServiceModules>();
        }
    }
}
