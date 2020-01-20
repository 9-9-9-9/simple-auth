using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Test.Shared.Utils;

namespace Test.SimpleAuth.Server.Support.Extensions
{
    public static class MockExtensions
    {
        public static FutureSetup<T> FSet<T>(this Mock<T> svc, Action<Mock<T>> act) where T : class
        {
            return new FutureSetup<T>(svc, act);
        }

        public static IReturnsResult<T> ReturnsTask<T>(this ISetup<T, Task> setup) where T : class
        {
            return setup.Returns(Task.CompletedTask);
        }

        public static IReturnsResult<T> ReturnsTask<T>(this ISetup<T, Task> setup, Exception ex) where T : class
        {
            return setup.Returns(Task.FromException(ex));
        }

        public static IReturnsResult<T> ReturnsTask<T>(this ISetup<T, Task> setup, IEnumerable<object> result)
            where T : class
        {
            return setup.Returns(Task.FromResult(result));
        }

        public static Mock<IServiceProvider> WithLogger<T>(this Mock<IServiceProvider> isp)
        {
            isp.With<ILogger<T>>(out var logger);
            
            logger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>) It.IsAny<object>())
            );
            
            return isp;
        }

        public static Mock<IServiceProvider> With<TMock>(this Mock<IServiceProvider> mock, out Mock<TMock> newMockObj)
            where TMock : class
        {
            newMockObj = MoqU.Of<TMock>();
            mock.Setup(x => x.GetService(typeof(TMock))).Returns(newMockObj.Object);
            return mock;
        }

        public static Mock<IServiceProvider> WithIn<TMock>(this Mock<IServiceProvider> mock, in TMock newMockObj)
            where TMock : class
        {
            mock.Setup(x => x.GetService(typeof(TMock))).Returns(newMockObj);
            return mock;
        }

        public class FutureSetup<T> where T : class
        {
            private Mock<T> _mock;
            private readonly Action<Mock<T>> _setupAct;
            private bool _alreadySetup;

            public T Object
            {
                get
                {
                    if (_mock == null)
                        _mock = new Mock<T>();
                    if (!_alreadySetup)
                        DoSetup();
                    return _mock.Object;
                }
            }

            public FutureSetup(Mock<T> mock, Action<Mock<T>> setupAct)
            {
                _mock = mock;
                _setupAct = setupAct;
            }

            public FutureSetup(Action<Mock<T>> setupAct) : this(null, setupAct)
            {
            }

            public void DoSetup()
            {
                if (_alreadySetup)
                    return;
                _setupAct(_mock);
                _alreadySetup = true;
            }
        }
    }
}