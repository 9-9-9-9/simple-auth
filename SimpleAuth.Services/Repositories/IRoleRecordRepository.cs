using System;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface IRoleRecordRepository : IRepository<PermissionRecord, Guid>
    {
    }

    public class RoleRecordRepository : Repository<PermissionRecord, Guid>, IRoleRecordRepository
    {
        public RoleRecordRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}