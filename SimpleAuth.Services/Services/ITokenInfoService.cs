using System;
using System.Threading.Tasks;
using SimpleAuth.Repositories;
using SimpleAuth.Services.Entities;
using TokenInfo = SimpleAuth.Shared.Domains.TokenInfo;

namespace SimpleAuth.Services
{
    public interface ITokenInfoService : IDomainService
    {
        Task<int> IncreaseVersionAsync(TokenInfo tokenInfo);
        int GetCurrentVersion(TokenInfo tokenInfo);
    }

    public class DefaultTokenInfoService : DomainService<ITokenInfoRepository, Entities.TokenInfo>,
        ITokenInfoService
    {
        private readonly ICachedTokenInfoRepository _cachedTokenInfoRepository;

        public DefaultTokenInfoService(IServiceProvider serviceProvider,
            ICachedTokenInfoRepository cachedTokenInfoRepository) : base(serviceProvider)
        {
            _cachedTokenInfoRepository = cachedTokenInfoRepository;
        }

        public async Task<int> IncreaseVersionAsync(TokenInfo tokenInfo)
        {
            var entity = await Repository
                .FindSingleAsync(x =>
                    x.Corp == tokenInfo.Corp
                    &&
                    x.App == tokenInfo.App
                );
            if (entity == default)
            {
                entity = new Entities.TokenInfo
                {
                    Corp = tokenInfo.Corp,
                    App = tokenInfo.App,
                    Version = 1
                }.WithRandomId();
                await Repository.CreateAsync(entity);
            }
            else
            {
                entity.Version += 1;
                await Repository.UpdateAsync(entity);
            }

            _cachedTokenInfoRepository.Clear(tokenInfo.Corp, tokenInfo.App);
            _cachedTokenInfoRepository.Push(entity);
            return entity.Version;
        }

        public int GetCurrentVersion(TokenInfo tokenInfo)
        {
            return _cachedTokenInfoRepository.Get(tokenInfo.Corp, tokenInfo.App)?.Version ?? 0;
        }
    }
}