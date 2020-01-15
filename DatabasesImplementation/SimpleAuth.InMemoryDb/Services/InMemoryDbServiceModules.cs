using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Services;

namespace SimpleAuth.InMemoryDb.Services
{
    public class InMemoryDbServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ISqlExceptionHandlerService, InMemoryDbExceptionHandlerService>();
        }
    }
}