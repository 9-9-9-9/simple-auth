using System;

namespace SimpleAuth.Shared.Exceptions
{
    public class HandledSqlException : SimpleAuthException
    {
        // ReSharper disable once MemberCanBeProtected.Global
        public HandledSqlException(string message) : base(message)
        {
        }

        public HandledSqlException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    public class ConstraintViolationException : HandledSqlException
    {
        public ConstraintViolationException(string message) : base(message)
        {
        }

        public ConstraintViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    public class ConcurrentUpdateException : HandledSqlException
    {
        public ConcurrentUpdateException(string message) : base(message)
        {
        }

        public ConcurrentUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}