using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleAuth.Core.DependencyInjection;

namespace Test.SimpleAuth.Core.Test.DependencyInjection
{
    public class TestRegistrableModules : BaseTestClass
    {
        protected override void RegisteredServices(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterModules<MyRegistrableModels>();
            base.RegisteredServices(serviceCollection);
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