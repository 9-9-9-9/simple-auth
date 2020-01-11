using System;
using System.Threading.Tasks;
using AdministratorConsole.Commands;
using ConsoleApps.Shared;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.Models;
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
                .AddSingleton(new SimpleAuthSettings
                {
                    TokenSettings = new SimpleAuthTokenSettings
                    {
                        MasterToken = "dbF5x5I+83EVOl8yApRhbrhUbnVdkWVf5yoxzwL5oO35dpLIV2BhngAbPyhpCrpYB+NL1PrzQOqslU5UbPiGNCEv3TxdLBt7lbSBgFvMAJ/EtIdj1JKxG5y3nJbI3F9xZ34NqcXqTnhCq4UVxk+Sb4L9zYcq59uZjAT6rYZqUk/DfsvunfZxnvpBMMqI1TbDy1py6a0mMbfTBzZuJdQ0+wdgm+R1F7en4pPBfqYTG8MD7fOmDLOkX/aHWxCSJ41J5EAIAWZlgNl5qRluzR8lfG5B7GLFJXzE85gjbqLHwgATSlZWi2RHYxYAA8SpQHrNPNPWC5QggFnFrmp0OSvcaw==",
                    },
                    SimpleAuthServerUrl = 
                        "http://localhost:5000"
                    //"http://standingtrust.com"
                    ,
                })
                .RegisterModules<AdministratorCommandModules>()
                .RegisterModules<BasicServiceModules>();
        }
    }
}