using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Sqlite.Services;

namespace SimpleAuth.Sqlite
{
    public class ProjectRegistrableModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterModules<SqliteServiceModules>();
        }
    }
}