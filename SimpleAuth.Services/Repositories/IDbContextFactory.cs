using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleAuth.Repositories
{
    public interface IDbContextFactory
    {
        DbContext CreateDbContext();
    }

    public class DefaultDbContextFactory : IDbContextFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DefaultDbContextFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public DbContext CreateDbContext()
        {
            return _serviceScopeFactory.CreateScope().ServiceProvider.GetService<SimpleAuthDbContext>();
        }
    }
}