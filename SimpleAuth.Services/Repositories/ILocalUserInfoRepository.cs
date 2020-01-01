using System;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface ILocalUserInfoRepository : IRepository<LocalUserInfo, Guid>
    {
    }

    public class LocalUserInfoRepository : Repository<LocalUserInfo, Guid>, ILocalUserInfoRepository
    {
        public LocalUserInfoRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}
