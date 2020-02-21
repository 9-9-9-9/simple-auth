using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Shared.DependencyInjection;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Models
{
    public static class SimpleAuthorizationClaimAdditionalExtensions
    {
        public static async Task<Claim> GenerateSimpleAuthClaimAsync(this PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim,
            IServiceResolver serviceProvider)
        {
            var authenticationInfoProvider = serviceProvider.GetService<IAuthenticationInfoProvider>();
            return await authenticationInfoProvider.GenerateSimpleAuthClaimAsync(packageSimpleAuthorizationClaim);
        }
        
        public static Task<Claim> GenerateSimpleAuthClaimAsync(this ResponseUserModel responseUserModel,
            IServiceResolver serviceProvider)
        {
            if (responseUserModel == null)
                throw new ArgumentNullException(nameof(responseUserModel));
            return new PackageSimpleAuthorizationClaim
            {
                UserId = responseUserModel.Id,
                Claims = (responseUserModel.ActiveRoles?.ToSimpleAuthorizationClaims()).OrEmpty().ToArray()
            }.GenerateSimpleAuthClaimAsync(serviceProvider);
        }
    }
}