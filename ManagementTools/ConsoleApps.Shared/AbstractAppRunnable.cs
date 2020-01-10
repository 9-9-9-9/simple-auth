using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;

namespace ConsoleApps.Shared
{
    public abstract class AbstractAppRunnable
    {
        public virtual async Task RunAsync()
        {
            IServiceCollection services = new ServiceCollection();
            services = RegisterServiceCollections(services);

            var serviceProvider = services.BuildServiceProvider();

            var commands = serviceProvider.GetServices<ICommand>();

            await DoWorkAsync(serviceProvider);
        }
        protected abstract IServiceCollection RegisterServiceCollections(IServiceCollection serviceCollection);

        protected abstract Task DoWorkAsync(IServiceProvider serviceProvider);
    }
}
