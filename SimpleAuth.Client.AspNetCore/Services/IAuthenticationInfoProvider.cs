using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleAuth.Client.Models;
using SimpleAuth.Core.Extensions;

namespace SimpleAuth.Client.AspNetCore.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IAuthenticationInfoProvider
    {
        Task<bool> IsAuthenticated(HttpContext httpContext);
        Task<ICollection<Claim>> GetClaimsAsync(HttpContext httpContext);
        Task<Claim> GetSimpleAuthClaimAsync(HttpContext httpContext);
        Claim GetSimpleAuthClaim(ICollection<Claim> claims);
        Task<Claim> GenerateSimpleAuthClaimAsync(ICollection<SimpleAuthorizationClaim> claims);
        Task<ICollection<SimpleAuthorizationClaim>> GetSimpleAuthClaimsAsync(Claim claim);
    }

    public class DefaultAuthenticationInfoProvider : IAuthenticationInfoProvider
    {
        private readonly IClaimTransformingService _claimTransformingService;

        public DefaultAuthenticationInfoProvider(IClaimTransformingService claimTransformingService)
        {
            _claimTransformingService = claimTransformingService;
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

        public Claim GetSimpleAuthClaim(ICollection<Claim> claims)
        {
            return claims.GetDefaultSimpleAuthClaim();
        }

        public async Task<Claim> GetSimpleAuthClaimAsync(HttpContext httpContext)
        {
            return GetSimpleAuthClaim(await GetClaimsAsync(httpContext));
        }

        public Task<Claim> GenerateSimpleAuthClaimAsync(ICollection<SimpleAuthorizationClaim> claims)
        {
            return _claimTransformingService.PackAsync(claims);
        }

        public async Task<ICollection<SimpleAuthorizationClaim>> GetSimpleAuthClaimsAsync(Claim claim)
        {
            if (claim == null)
                return Enumerable.Empty<SimpleAuthorizationClaim>().ToList();

            return await _claimTransformingService.UnpackAsync(claim);
        }
    }
}