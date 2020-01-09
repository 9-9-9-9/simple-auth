using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleAuth.Client.Models;

namespace SimpleAuth.Client.AspNetCore.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IAuthenticationInfoProvider
    {
        Task<bool> IsAuthenticated(HttpContext httpContext);
        Task<IEnumerable<Claim>> GetClaims(HttpContext httpContext);
        Task<Claim> GetSimpleAuthClaimAsync(HttpContext httpContext);
        Claim GetSimpleAuthClaim(IEnumerable<Claim> claims);
        Task<Claim> GenerateSimpleAuthClaimAsync(IEnumerable<SimpleAuthorizationClaim> claims);
        Task<IEnumerable<SimpleAuthorizationClaim>> GetSimpleAuthClaimsAsync(Claim claim);
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

        public async Task<Claim> GetSimpleAuthClaimAsync(HttpContext httpContext)
        {
            return GetSimpleAuthClaim(await GetClaims(httpContext));
        }

        public Claim GetSimpleAuthClaim(IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(x => x.Type == SimpleAuthDefaults.ClaimType);
        }

        public async Task<Claim> GenerateSimpleAuthClaimAsync(IEnumerable<SimpleAuthorizationClaim> claims)
        {
            var randomValue = Guid.NewGuid().ToString();
            await _claimCachingService.SaveClaimsAsync(randomValue, claims);
            return new Claim(SimpleAuthDefaults.ClaimType, randomValue);
        }

        public async Task<IEnumerable<SimpleAuthorizationClaim>> GetSimpleAuthClaimsAsync(Claim claim)
        {
            if (claim?.Type != SimpleAuthDefaults.ClaimType)
                throw new ArgumentException($"{nameof(claim)}: is not type '{SimpleAuthDefaults.ClaimType}'");
            return (await _claimCachingService.GetClaimsAsync(claim.Value));
        }
    }
}