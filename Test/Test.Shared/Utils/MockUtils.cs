using System;
using Moq;
using Test.SimpleAuth.Server.Support.Extensions;

namespace Test.Shared.Utils
{
    public static class MoqU
    {
        public static Mock<T> Of<T>(MockBehavior mockBehavior = MockBehavior.Strict) where T : class
        {
            return new Mock<T>(mockBehavior);
        }

        public static Mock<IServiceProvider> OfServiceProvider(MockBehavior mockBehavior = MockBehavior.Strict)
        {
            return Of<IServiceProvider>();
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<T>(MockBehavior mockBehavior = MockBehavior.Strict)
        {
            return Of<IServiceProvider>()
                    .WithLogger<T>()
                ;
        }
    }
}