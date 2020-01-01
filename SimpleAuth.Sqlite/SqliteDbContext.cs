using Microsoft.EntityFrameworkCore;
using SimpleAuth.Repositories;

namespace SimpleAuth
{
    public abstract class SqliteDbContext : SimpleAuthDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(@"Data Source=sa.server.db");
    }
}