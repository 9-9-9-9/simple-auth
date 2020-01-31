using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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

        public static Mock<IServiceProvider> OfServiceProviderFor<TClass, T2>(
            MockBehavior mockBehavior = MockBehavior.Strict)
            where T2 : class
        {
            return OfServiceProviderFor<TClass>(mockBehavior)
                    .With<T2>(out _)
                ;
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<TClass, T2, T3>(
            MockBehavior mockBehavior = MockBehavior.Strict)
            where T2 : class
            where T3 : class
        {
            return OfServiceProviderFor<TClass, T2>(mockBehavior)
                    .With<T3>(out _)
                ;
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<TClass, T2, T3, T4>(
            MockBehavior mockBehavior = MockBehavior.Strict)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return OfServiceProviderFor<TClass, T2, T3>(mockBehavior)
                    .With<T4>(out _)
                ;
        }

        public static Mock<IServiceProvider> OfServiceProviderFor<TClass, T2, T3, T4, T5>(
            MockBehavior mockBehavior = MockBehavior.Strict)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return OfServiceProviderFor<TClass, T2, T3, T4>(mockBehavior)
                    .With<T5>(out _)
                ;
        }

        public static Mock<HttpContext> OfHttpContext(
            out Mock<IServiceProvider> mockServiceProvider,
            out Mock<HttpRequest> mockHttpRequest,
            out Mock<HttpResponse> mockHttpResponse,
            out Mock<IServiceScopeFactory> mockServiceScopeFactory,
            out Mock<IServiceScope> mockServiceScope,
            in HeaderDictionary requestHeaders = null,
            in HeaderDictionary responseHeaders = null)
        {
            var ctx = Of<HttpContext>();
            mockHttpRequest = Of<HttpRequest>();
            mockHttpResponse = Of<HttpResponse>();
            mockServiceProvider = OfServiceProvider();
            mockServiceScope = Of<IServiceScope>();
            mockServiceScopeFactory = Of<IServiceScopeFactory>();

            ctx.SetupGet(x => x.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(mockServiceScopeFactory.Object);
            mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);
            mockServiceScope.SetupGet(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            ctx.SetupGet(x => x.Request).Returns(mockHttpRequest.Object);
            ctx.SetupGet(x => x.Response).Returns(mockHttpResponse.Object);
            if (requestHeaders != default)
                mockHttpRequest.SetupGet(x => x.Headers).Returns(requestHeaders);
            if (responseHeaders != default)
                mockHttpResponse.SetupGet(x => x.Headers).Returns(responseHeaders);

            return ctx;
        }
    }
}