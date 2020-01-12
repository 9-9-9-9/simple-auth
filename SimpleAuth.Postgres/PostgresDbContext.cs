using System.IO;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Repositories;

namespace SimpleAuth
{
    public abstract class PostgresDbContext : SimpleAuthDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseNpgsql(File.ReadAllText("/opt/secret/simple-auth/postgresql.txt"));
    }
}