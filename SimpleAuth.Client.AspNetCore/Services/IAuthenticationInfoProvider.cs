using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleAuth.Client.Models;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;

namespace SimpleAuth.Client.AspNetCore.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IAuthenticationInfoProvider
    {
        Task<bool> IsAuthenticated(HttpContext httpContext);
        Task<ICollection<Claim>> GetClaimsAsync(HttpContext httpContext);
        Task<Claim> GetSimpleAuthClaimAsync(HttpContext httpContext);
        Claim GetSimpleAuthClaim(IEnumerable<Claim> claims);
        Task<Claim> GenerateSimpleAuthClaimAsync(IEnumerable<SimpleAuthorizationClaim> claims);
        Task<ICollection<SimpleAuthorizationClaim>> GetSimpleAuthClaimsAsync(Claim claim);
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

        public async Task<ICollection<Claim>> GetClaimsAsync(HttpContext httpContext)
        {
            await Task.CompletedTask;
            return httpContext.User.Claims.OrEmpty().ToList();
        }

        public Claim GetSimpleAuthClaim(IEnumerable<Claim> claims)
        {
            return claims.GetDefaultSimpleAuthClaim();
        }

        public async Task<Claim> GetSimpleAuthClaimAsync(HttpContext httpContext)
        {
            return GetSimpleAuthClaim(await GetClaimsAsync(httpContext));
        }

        public async Task<Claim> GenerateSimpleAuthClaimAsync(IEnumerable<SimpleAuthorizationClaim> claims)
        {
            var randomValue = Guid.NewGuid().ToString();
            await _claimCachingService.SaveClaimsAsync(randomValue, claims);
            return new Claim(SimpleAuthDefaults.ClaimType, randomValue, nameof(SimpleAuthorizationClaim), Constants.Identity.Issuer);
        }

        public async Task<ICollection<SimpleAuthorizationClaim>> GetSimpleAuthClaimsAsync(Claim claim)
        {
            if (claim == null)
                return Enumerable.Empty<SimpleAuthorizationClaim>().ToList();
            
            if (claim?.Type != SimpleAuthDefaults.ClaimType)
                throw new ArgumentException($"{nameof(claim)}: is not type '{SimpleAuthDefaults.ClaimType}'");
            
            return (await _claimCachingService.GetClaimsAsync(claim.Value)).OrEmpty().ToList();
        }
    }
}