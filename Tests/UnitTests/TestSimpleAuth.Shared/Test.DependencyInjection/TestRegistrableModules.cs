using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleAuth.Shared.DependencyInjection;

namespace Test.SimpleAuth.Shared.Test.DependencyInjection
{
    public class TestRegistrableModules
    {
        protected IServiceProvider Prepare()
        {
            var services = new ServiceCollection();
            RegisteredServices(services);
            return services.BuildServiceProvider();
        }
        
        protected void RegisteredServices(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterModules<MyRegistrableModels>();
        }

        [Test]
        public void RegistrableModules()
        {
            var serviceProvider = Prepare();
            var myService = serviceProvider.GetRequiredService<IMyService>();
            Assert.NotNull(myService);
            Assert.AreEqual("Hi!", myService.SayHello());
        }

        class MyRegistrableModels : RegistrableModules
        {
            public override void RegisterModules(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<IMyService, MyService>();
            }
        }

        interface IMyService
        {
            string SayHello();
        }

        class MyService : IMyService
        {
            public string SayHello() => "Hi!";
        }
    }
}