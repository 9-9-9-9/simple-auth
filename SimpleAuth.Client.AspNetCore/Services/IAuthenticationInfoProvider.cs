using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleAuth.Client.Models;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public interface IAuthenticationInfoProvider
    {
        Task<bool> IsAuthenticated(HttpContext httpContext);
        Task<IEnumerable<Claim>> GetClaims(HttpContext httpContext);
        Task<Claim> GetSimpleAuthClaim(HttpContext httpContext);
        Task<Claim> GenerateSimpleAuthClaimAsync(IEnumerable<SimpleAuthorizationClaim> claims);
    }

    public class DefaultAuthenticationInfoProvider : IAuthenticationInfoProvider
    {
        private readonly IClaimCachingService _claimCachingService;

        public DefaultAuthenticationInfoProvider(IClaimCachingService claimCachingService)
        {
            _claimCachingService = claimCachingService;
        }

        public Task<bool> IsAuthenticated(HttpContext httpContext)
        {
            return Task.FromResult(httpContext.User.Identity.IsAuthenticated);
        }

        public Task<IEnumerable<Claim>> GetClaims(HttpContext httpContext)
        {
            return Task.FromResult(httpContext.User.Claims);
        }

        public async Task<Claim> GetSimpleAuthClaim(HttpContext httpContext)
        {
            var claims = await GetClaims(httpContext);
            return claims.FirstOrDefault(x => x.Type == SimpleAuthDefaults.ClaimType);
        }

        public async Task<Claim> GenerateSimpleAuthClaimAsync(IEnumerable<SimpleAuthorizationClaim> claims)
        {
            var randomValue = Guid.NewGuid().ToString();
            await _claimCachingService.SaveClaimsAsync(randomValue, claims);
            return new Claim(SimpleAuthDefaults.ClaimType, randomValue);
        }
    }
}