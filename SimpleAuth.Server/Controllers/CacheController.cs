using System;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Middlewares;

namespace SimpleAuth.Server.Controllers
{
    [Route("cache")]
    [RequireAppToken]
    public class CacheController : BaseController
    {
        private readonly ICachedTokenInfoRepository _cachedTokenInfoRepository;

        public CacheController(IServiceProvider serviceProvider,
            ICachedTokenInfoRepository cachedTokenInfoRepository) :
            base(serviceProvider)
        {
            _cachedTokenInfoRepository = cachedTokenInfoRepository;
        }

        [HttpGet, HttpPost, HttpDelete, Route("clear/token-info")]
        public void ClearCacheTokenInfo()
        {
            _cachedTokenInfoRepository.Clear(RequestAppHeaders.Corp, RequestAppHeaders.App); // App token
            _cachedTokenInfoRepository.Clear(RequestAppHeaders.Corp, string.Empty); // Corp token
        }
    }
}