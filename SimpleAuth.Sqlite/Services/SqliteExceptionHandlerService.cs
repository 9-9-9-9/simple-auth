using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Sqlite.Services
{
    public class SqliteExceptionHandlerService : AbstractSqlExceptionHandlerService
    {
        public override Exception TryTransform(Exception exception)
        {
            if (exception is DbUpdateConcurrencyException concurrencyEx)
                return new ConcurrentUpdateException($"Please try again later", concurrencyEx);

            if (!(exception is DbUpdateException dbUpdateEx))
                return exception;

            if (dbUpdateEx.InnerException != null)
            {
                if (dbUpdateEx.InnerException is SqliteException sqlException)
                {
                    switch (sqlException.SqliteErrorCode)
                    {
                        case 19: // Unique constraint error
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