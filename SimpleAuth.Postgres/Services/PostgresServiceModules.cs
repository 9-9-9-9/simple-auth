using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Services;

namespace SimpleAuth.Postgres.Services
{
    public class PostgresServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ISqlExceptionHandlerService, PostgresExceptionHandlerService>();
        }
    }
}