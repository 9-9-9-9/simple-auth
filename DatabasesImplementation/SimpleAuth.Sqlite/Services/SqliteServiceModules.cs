using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.DependencyInjection;
using SimpleAuth.Services;

namespace SimpleAuth.Sqlite.Services
{
    public class SqliteServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ISqlExceptionHandlerService, SqliteExceptionHandlerService>();
        }
    }
}