using System.Collections.Generic;
using SimpleAuth.Core.Extensions;

namespace SimpleAuth.Shared.Exceptions
{
    public class EntityNotExistsException : SimpleAuthException
    {
        public EntityNotExistsException(string missing) : base(missing)
        {
        }
        
        public EntityNotExistsException(IEnumerable<object> missing) : base(string.Join(',', missing.DropNull()))
        {
        }
    }
}