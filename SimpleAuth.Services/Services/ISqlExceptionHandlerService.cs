using System;

namespace SimpleAuth.Services
{
    public interface ISqlExceptionHandlerService
    {
        void RethrowException(Exception exception);
        Exception TryTransform(Exception exception);
    }

    public abstract class AbstractSqlExceptionHandlerService : ISqlExceptionHandlerService
    {
        public virtual void RethrowException(Exception exception)
        {
            throw TryTransform(exception);
        }

        public abstract Exception TryTransform(Exception exception);
    }
}