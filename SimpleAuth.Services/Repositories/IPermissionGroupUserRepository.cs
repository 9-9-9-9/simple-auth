using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface IPermissionGroupUserRepository : IRepository<PermissionGroupUser>
    {
    }
    
    public class PermissionGroupUserRepository : Repository<PermissionGroupUser>, IPermissionGroupUserRepository
    {
        public PermissionGroupUserRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }

        protected override IQueryable<PermissionGroupUser> Include(DbSet<PermissionGroupUser> dbSet)
        {
            return base.Include(dbSet)
                .Include(x => x.User)
                .Include(x => x.PermissionGroup);
        }
    }
}