using System;
using Microsoft.Extensions.DependencyInjection;

namespace Test.SimpleAuth.Core
{
    public abstract class BaseTestClass
    {
        protected virtual IServiceProvider Prepare()
        {
            var services = new ServiceCollection();
            RegisteredServices(services);
            return services.BuildServiceProvider();
        }

        protected virtual void RegisteredServices(IServiceCollection serviceCollection)
        {
        }
    }
}