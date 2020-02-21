using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.DependencyInjection;
using Test.Shared;

namespace Test.SimpleAuth.Server
{
    public class BaseTestServer : BaseTestClass
    {
        protected override void RegisteredServices(IServiceCollection serviceCollection)
        {
            base.RegisteredServices(serviceCollection);
            serviceCollection.RegisterModules<global::SimpleAuth.Server.Services.ServiceModules>();
        }
    }
}