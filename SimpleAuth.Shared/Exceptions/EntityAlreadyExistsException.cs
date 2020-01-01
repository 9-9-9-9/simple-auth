namespace SimpleAuth.Shared.Exceptions
{
    public class EntityAlreadyExistsException : SimpleAuthException
    {
        public EntityAlreadyExistsException(string message) : base(message)
        {
        }
    }
}