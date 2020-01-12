using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Postgres.Services;

namespace SimpleAuth.Postgres
{
    public class ProjectRegistrableModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterModules<PostgresServiceModules>();
        }
    }
}