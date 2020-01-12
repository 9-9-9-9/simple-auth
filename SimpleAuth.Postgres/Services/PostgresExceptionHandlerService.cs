using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SimpleAuth.Services;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Postgres.Services
{
    public class PostgresExceptionHandlerService : AbstractSqlExceptionHandlerService
    {
        public override Exception TryTransform(Exception exception)
        {
            if (exception is DbUpdateConcurrencyException concurrencyEx)
                return new ConcurrentUpdateException($"Please try again later", concurrencyEx);

            if (!(exception is DbUpdateException dbUpdateEx))
                return exception;

            if (dbUpdateEx.InnerException != null)
            {
                if (dbUpdateEx.InnerException is PostgresException sqlException)
                {
                    switch (sqlException.ErrorCode)
                    {
                        case 23505: // Unique constraint error
                            //case 547: // Constraint check violation
                            //case 2601: // Duplicated key row error
                            // Constraint violation exception
                            // A custom exception of yours for concurrency issues
                            return new ConstraintViolationException("Constrain violated", sqlException);
                    }
                }
            }
            
            return new HandledSqlException(dbUpdateEx.Message, dbUpdateEx.InnerException);
        }
    }
}