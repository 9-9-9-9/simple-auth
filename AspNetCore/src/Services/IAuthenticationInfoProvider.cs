using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Client.AspNetCore.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IAuthenticationInfoProvider
    {
        Task<bool> IsAuthenticated(HttpContext httpContext);
        Task<ICollection<Claim>> GetClaimsAsync(HttpContext httpContext);
        Task<Claim> GetSimpleAuthClaimAsync(HttpContext httpContext);
        Claim GetSimpleAuthClaim(ICollection<Claim> claims);
        Task<Claim> GenerateSimpleAuthClaimAsync(PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim);
        Task<PackageSimpleAuthorizationClaim> GetPackageSimpleAuthClaimAsync(Claim claim);
        /// <summary>
        /// Verify if user has required claims. If user doesn't have any of required claims, an <see cref="DataVerificationMismatchException"/> will be thrown
        /// </summary>
        Task AuthorizeAsync(HttpContext httpContext, ICollection<SimpleAuthorizationClaim> requiredClaims);
    }

    public class DefaultAuthenticationInfoProvider : IAuthenticationInfoProvider
    {
        private readonly IClaimTransformingService _claimTransformingService;
        private readonly IUserAuthService _userAuthService;
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;

        public DefaultAuthenticationInfoProvider(IClaimTransformingService claimTransformingService,
            IUserAuthService userAuthService, ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _claimTransformingService = claimTransformingService;
            _userAuthService = userAuthService;
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
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

        public Task<Claim> GenerateSimpleAuthClaimAsync(PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim)
        {
            return _claimTransformingService.PackAsync(packageSimpleAuthorizationClaim);
        }

        public async Task<PackageSimpleAuthorizationClaim> GetPackageSimpleAuthClaimAsync(Claim claim)
        {
            if (claim == null)
                return default;

            return await _claimTransformingService.UnpackAsync(claim);
        }

        /// <inheritdoc />
        public async Task AuthorizeAsync(HttpContext httpContext, ICollection<SimpleAuthorizationClaim> requiredClaims)
        {
            var packageSimpleAuthorizationClaim = httpContext.GetUserPackageSimpleAuthorizationClaimFromContext();

            if (_simpleAuthConfigurationProvider.LiveChecking)
                await PerformLiveCheckingPermission(httpContext, packageSimpleAuthorizationClaim, requiredClaims);
            else
                await PerformLocalCheckingPermission(httpContext, packageSimpleAuthorizationClaim, requiredClaims);
        }

        private async Task PerformLocalCheckingPermission(HttpContext httpContext,
            PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim,
            ICollection<SimpleAuthorizationClaim> requiredClaims)
        {
            var userClaims = packageSimpleAuthorizationClaim.ClaimsOrEmpty;

            if (!userClaims.IsAny())
                throw new DataVerificationMismatchException("User doesn't have any permission");

            var missingClaims = userClaims.GetMissingClaims(requiredClaims)
                .OrEmpty()
                .ToArray();

            if (missingClaims.Any())
                throw new DataVerificationMismatchException($"Require {missingClaims[0].ClientRoleModel}");

            await Task.CompletedTask;
        }

        private async Task PerformLiveCheckingPermission(HttpContext httpContext,
            PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim,
            ICollection<SimpleAuthorizationClaim> requiredClaims)
        {
            if (packageSimpleAuthorizationClaim.UserId.IsBlank())
                throw new DataVerificationMismatchException(
                    $"Can't find user id from {nameof(PackageSimpleAuthorizationClaim)}");

            var missingRoles = await _userAuthService.GetMissingRolesAsync(packageSimpleAuthorizationClaim.UserId,
                new RoleModels
                {
                    Roles = requiredClaims.Select(x => x.ClientRoleModel.ToRole().Cast()).ToArray()
                });

            if (missingRoles.Any())
            {
                var firstRoleModel = missingRoles.First();
                RoleUtils.Parse(firstRoleModel.Role, firstRoleModel.Permission, out var clientRoleModel);
                throw new DataVerificationMismatchException($"Require {clientRoleModel}");
            }
        }
    }
}