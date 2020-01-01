using System;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface IRoleRecordRepository : IRepository<RoleRecord, Guid>
    {
    }

    public class RoleRecordRepository : Repository<RoleRecord, Guid>, IRoleRecordRepository
    {
        public RoleRecordRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}