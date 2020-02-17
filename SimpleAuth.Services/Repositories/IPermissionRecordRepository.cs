using System;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface IPermissionRecordRepository : IRepository<PermissionRecord, Guid>
    {
    }

    public class PermissionRecordRepository : Repository<PermissionRecord, Guid>, IPermissionRecordRepository
    {
        public PermissionRecordRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}