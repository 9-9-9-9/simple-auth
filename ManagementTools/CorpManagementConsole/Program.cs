using System;
using System.Threading.Tasks;
using ConsoleApps.Shared;
using CorpManagementConsole.Commands;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.DependencyInjection;

namespace CorpManagementConsole
{
    internal static class Program
    {
        internal static async Task<int> Main()
        {
            try
            {
                await new CorpManagementApp().RunAsync();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
        }
    }

    internal class CorpManagementApp : AbstractAppRunnable
    {
        protected override bool AllowSwitchingDefaultApp => false;

        protected override IServiceCollection RegisterServiceCollections(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .RegisterModules<CorpManagementCommandModules>()
                .RegisterModules<BasicServiceModules>();
        }
    }
}
