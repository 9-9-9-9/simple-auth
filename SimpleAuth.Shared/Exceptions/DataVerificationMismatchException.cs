namespace SimpleAuth.Shared.Exceptions
{
    public class DataVerificationMismatchException : SimpleAuthSecurityException
    {
        public DataVerificationMismatchException(string message) : base(message)
        {
        }
    }
}