using System;
using System.Threading.Tasks;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Services.Entities;

namespace SimpleAuth.Repositories
{
    public interface ICachedTokenInfoRepository
    {
        Task PushAsync(TokenInfo tokenInfo);
        Task<TokenInfo> GetAsync(string corp, string app);
        Task ClearAsync(string corp, string app);
    }

    public class CachedTokenInfoRepository : ICachedTokenInfoRepository
    {
        private readonly ICachedRepository<TokenInfo> _memoryCachedRepository;

        public CachedTokenInfoRepository(ICachedRepository<TokenInfo> memoryCachedRepository)
        {
            _memoryCachedRepository = memoryCachedRepository;
        }

        public Task PushAsync(TokenInfo tokenInfo)
        {
            if (tokenInfo == null)
                throw new ArgumentNullException(nameof(tokenInfo));
            return _memoryCachedRepository.PushAsync(tokenInfo, BuildKey(tokenInfo), tokenInfo.Corp, tokenInfo.App);
        }

        public Task<TokenInfo> GetAsync(string corp, string app)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));
            return _memoryCachedRepository.GetAsync(BuildKey(new TokenInfo
            {
                Corp = corp,
                App = app
            }), corp, app);
        }

        public Task ClearAsync(string corp, string app)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));
            return _memoryCachedRepository.ClearAsync(corp, app);
        }

        private string BuildKey(TokenInfo tokenInfo)
        {
            if (tokenInfo.Corp.IsBlank())
                throw new ArgumentException($"Property {nameof(TokenInfo.Corp)} of {nameof(tokenInfo)} is blank");
            return $"{tokenInfo.Corp}@{tokenInfo.App.Or(string.Empty)}";
        }
    }
}