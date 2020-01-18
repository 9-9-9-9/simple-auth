using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Middlewares;
#pragma warning disable 1591

namespace SimpleAuth.Server.Controllers
{
    [Route("cache")]
    [RequireAppToken]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CacheController : BaseController
    {
        private readonly ICachedTokenInfoRepository _cachedTokenInfoRepository;

        public CacheController(IServiceProvider serviceProvider,
            ICachedTokenInfoRepository cachedTokenInfoRepository) :
            base(serviceProvider)
        {
            _cachedTokenInfoRepository = cachedTokenInfoRepository;
        }

        [HttpGet, Route("clear/token-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public void ClearCacheTokenInfo()
        {
            _cachedTokenInfoRepository.Clear(RequestAppHeaders.Corp, RequestAppHeaders.App); // App token
            _cachedTokenInfoRepository.Clear(RequestAppHeaders.Corp, string.Empty); // Corp token
        }
    }
}