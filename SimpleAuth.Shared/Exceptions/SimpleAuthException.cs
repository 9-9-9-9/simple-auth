using System;

namespace SimpleAuth.Shared.Exceptions
{
    public class SimpleAuthException : Exception
    {
        public SimpleAuthException(string message) : base(message)
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public SimpleAuthException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}