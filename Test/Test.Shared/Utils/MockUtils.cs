using System;
using Moq;
using Test.SimpleAuth.Server.Support.Extensions;

namespace Test.Shared.Utils
{
    public static class Mu // Mock Utilities
    {
        public static Mock<T> Of<T>(MockBehavior mockBehavior = MockBehavior.Strict) where T : class
        {
            return new Mock<T>(mockBehavior);
        }

        public static Mock<IServiceProvider> OfServiceProvider(MockBehavior mockBehavior = MockBehavior.Strict)
        {
            return Of<IServiceProvider>(mockBehavior);
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<T>(MockBehavior mockBehavior = MockBehavior.Strict)
        {
            return Of<IServiceProvider>(mockBehavior)
                    .WithLogger<T>()
                ;
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<TLogger, T2>(
            MockBehavior mockBehavior = MockBehavior.Strict)
            where T2 : class
        {
            return OfServiceProviderFor<TLogger>(mockBehavior)
                    .With<T2>(out _)
                ;
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<TLogger, T2, T3>(
            MockBehavior mockBehavior = MockBehavior.Strict)
            where T2 : class
            where T3 : class
        {
            return OfServiceProviderFor<TLogger, T2>(mockBehavior)
                    .With<T3>(out _)
                ;
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<TLogger, T2, T3, T4>(
            MockBehavior mockBehavior = MockBehavior.Strict)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return OfServiceProviderFor<TLogger, T2, T3>(mockBehavior)
                    .With<T4>(out _)
                ;
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<TLogger, T2, T3, T4, T5>(
            MockBehavior mockBehavior = MockBehavior.Strict)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return OfServiceProviderFor<TLogger, T2, T3, T4>(mockBehavior)
                    .With<T5>(out _)
                ;
        }
    }
}