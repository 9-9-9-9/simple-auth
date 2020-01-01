namespace SimpleAuth.Shared.Exceptions
{
    public class ValidationException : SimpleAuthException
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}