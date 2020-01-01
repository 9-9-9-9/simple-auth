using Microsoft.EntityFrameworkCore;
using SimpleAuth.Repositories;

namespace SimpleAuth.InMemoryDb
{
    public abstract class InMemoryDbContext : SimpleAuthDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseInMemoryDatabase(databaseName:"public");
    }
}