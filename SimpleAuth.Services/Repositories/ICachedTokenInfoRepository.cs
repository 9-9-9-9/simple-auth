using SimpleAuth.Core.Extensions;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface ICachedTokenInfoRepository : ICachedRepository<TokenInfo>
    {
        void Push(TokenInfo tokenInfo);
        TokenInfo Get(string corp, string app);
    }

    public class CachedTokenInfoRepository : MemoryCachedRepository<TokenInfo>, ICachedTokenInfoRepository
    {
        public void Push(TokenInfo tokenInfo)
        {
            Push(tokenInfo, BuildKey(tokenInfo), tokenInfo.Corp, tokenInfo.App);
        }

        public TokenInfo Get(string corp, string app)
        {
            return Get(BuildKey(new TokenInfo
            {
                Corp = corp,
                App = app
            }), corp, app);
        }

        private string BuildKey(TokenInfo tokenInfo) => $"{tokenInfo.Corp}@{tokenInfo.App.Or(string.Empty)}";
    }
}