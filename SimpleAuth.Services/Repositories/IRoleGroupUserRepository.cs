using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface IRoleGroupUserRepository : IRepository<PermissionGroupUser>
    {
    }
    
    public class RoleGroupUserRepository : Repository<PermissionGroupUser>, IRoleGroupUserRepository
    {
        public RoleGroupUserRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
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