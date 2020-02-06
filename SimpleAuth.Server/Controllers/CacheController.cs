using System;
using System.Threading.Tasks;
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
        public async Task<IActionResult> ClearCacheTokenInfo()
        {
            await _cachedTokenInfoRepository.ClearAsync(RequestAppHeaders.Corp, RequestAppHeaders.App); // App token
            await _cachedTokenInfoRepository.ClearAsync(RequestAppHeaders.Corp, string.Empty); // Corp token

            return Ok();
        }
    }
}