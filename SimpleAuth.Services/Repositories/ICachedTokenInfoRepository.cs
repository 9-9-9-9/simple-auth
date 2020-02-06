using SimpleAuth.Core.Extensions;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface ICachedTokenInfoRepository
    {
        void Push(TokenInfo tokenInfo);
        TokenInfo Get(string corp, string app);
        void Clear(string corp, string app);
    }

    public class CachedTokenInfoRepository : ICachedTokenInfoRepository
    {
        private readonly ICachedRepository<TokenInfo> _memoryCachedRepository;

        public CachedTokenInfoRepository(ICachedRepository<TokenInfo> memoryCachedRepository)
        {
            _memoryCachedRepository = memoryCachedRepository;
        }

        public void Push(TokenInfo tokenInfo)
        {
            _memoryCachedRepository.Push(tokenInfo, BuildKey(tokenInfo), tokenInfo.Corp, tokenInfo.App);
        }

        public TokenInfo Get(string corp, string app)
        {
            return _memoryCachedRepository.Get(BuildKey(new TokenInfo
            {
                Corp = corp,
                App = app
            }), corp, app);
        }

        public void Clear(string corp, string app)
        {
            _memoryCachedRepository.Clear(corp, app);
        }

        private string BuildKey(TokenInfo tokenInfo) => $"{tokenInfo.Corp}@{tokenInfo.App.Or(string.Empty)}";
    }
}