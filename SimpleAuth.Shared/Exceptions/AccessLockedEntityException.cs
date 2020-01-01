using System.Collections.Generic;
using SimpleAuth.Core.Extensions;

namespace SimpleAuth.Shared.Exceptions
{
    public class AccessLockedEntityException : SimpleAuthException
    {
        public AccessLockedEntityException(string message) : base(message)
        {
        }
        
        public AccessLockedEntityException(IEnumerable<object> entities) : base(string.Join(',', entities.DropNull()))
        {
        }
    }
}