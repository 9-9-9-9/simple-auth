using System;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface ITokenInfoRepository : IRepository<TokenInfo, Guid>
    {
    }

    public class TokenInfoRepository : Repository<TokenInfo, Guid>, ITokenInfoRepository
    {
        public TokenInfoRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
        {
        }
    }
}