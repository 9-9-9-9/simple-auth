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
        private readonly ICachedUserRolesRepository _cachedUserRolesRepository;
        private readonly ICachedTokenInfoRepository _cachedTokenInfoRepository;

        public CacheController(IServiceProvider serviceProvider, ICachedUserRolesRepository cachedUserRolesRepository,
            ICachedTokenInfoRepository cachedTokenInfoRepository) :
            base(serviceProvider)
        {
            _cachedUserRolesRepository = cachedUserRolesRepository;
            _cachedTokenInfoRepository = cachedTokenInfoRepository;
        }

        [HttpGet, HttpPost, HttpDelete, Route("clear/users/roles")]
        public void ClearCacheUsersRoles()
        {
            _cachedUserRolesRepository.Clear(RequestAppHeaders.Corp, RequestAppHeaders.App);
        }

        [HttpGet, HttpPost, HttpDelete, Route("clear/token-info")]
        public void ClearCacheTokenInfo()
        {
            _cachedTokenInfoRepository.Clear(RequestAppHeaders.Corp, RequestAppHeaders.App); // App token
            _cachedTokenInfoRepository.Clear(RequestAppHeaders.Corp, string.Empty); // Corp token
        }
    }
}