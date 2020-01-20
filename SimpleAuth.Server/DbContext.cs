using System;

namespace SimpleAuth.Server
{
    /// <summary>
    /// DB Context of this project
    /// </summary>
    public class DbContext : PostgresDbContext
    {
        /// <inheritdoc />
        public DbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}