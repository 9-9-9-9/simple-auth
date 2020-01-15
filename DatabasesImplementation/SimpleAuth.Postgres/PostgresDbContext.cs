using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleAuth.Repositories;

namespace SimpleAuth
{
    public abstract class PostgresDbContext : SimpleAuthDbContext
    {
        private readonly ILoggerFactory _loggerFactory;

        protected PostgresDbContext(IServiceProvider serviceProvider)
        {
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .UseLoggerFactory(_loggerFactory)
                .UseNpgsql(File.ReadAllText("/opt/secret/simple-auth/postgresql.txt"));
    }
}