using System;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.InMemoryDb.Services
{
    public class InMemoryDbExceptionHandlerService : AbstractSqlExceptionHandlerService
    {
        public override Exception TryTransform(Exception exception)
        {
            if (exception is DbUpdateConcurrencyException concurrencyEx)
                return new ConcurrentUpdateException($"Please try again later", concurrencyEx);

            if (exception is ArgumentException argEx && argEx.Message?.Contains("An item with the same key has already been added. Key:", StringComparison.InvariantCultureIgnoreCase) == true)
                return new ConstraintViolationException("Constrain violated", argEx);
            
            if (!(exception is DbUpdateException dbUpdateEx))
                return exception;

            return new HandledSqlException(dbUpdateEx.Message, dbUpdateEx.InnerException);
        }
    }
}