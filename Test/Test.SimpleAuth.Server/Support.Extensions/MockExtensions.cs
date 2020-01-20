using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using SimpleAuth.Server.Controllers;

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

        public static Mock<IServiceProvider> SetupLogger<T>(this Mock<IServiceProvider> isp, Mock<ILogger<T>> mockLogger)
        {
            isp.Setup(x => x.GetService(typeof(ILogger<T>))).Returns(mockLogger.Object);
            return isp;
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