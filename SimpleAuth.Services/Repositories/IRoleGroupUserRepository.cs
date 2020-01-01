using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface IRoleGroupUserRepository : IRepository<RoleGroupUser>
    {
    }
    
    public class RoleGroupUserRepository : Repository<RoleGroupUser>, IRoleGroupUserRepository
    {
        public RoleGroupUserRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }

        protected override IQueryable<RoleGroupUser> Include(DbSet<RoleGroupUser> dbSet)
        {
            return base.Include(dbSet)
                .Include(x => x.User)
                .Include(x => x.RoleGroup);
        }
    }
}