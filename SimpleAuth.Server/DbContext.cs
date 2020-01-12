using System;

namespace SimpleAuth.Server
{
    public class DbContext : PostgresDbContext
    {
        public DbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}