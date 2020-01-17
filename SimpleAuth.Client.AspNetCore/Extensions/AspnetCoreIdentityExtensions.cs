using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleAuth.Client.Models;
using SimpleAuth.Shared.Models;

namespace Microsoft.AspNetCore.Identity
{
    public static class AspnetCoreIdentityExtensions
    {
        public static async Task<IdentityResult> RefreshClaimByNameAsync<T>(this UserManager<T> userManager,
            string userName,
            ResponseUserModel responseUserModel, IServiceProvider serviceProvider)
            where T : class
        {
            return await userManager.RefreshClaimByUserAsync(
                await userManager.FindByNameAsync(userName),
                responseUserModel,
                serviceProvider);
        }

        public static async Task<IdentityResult> RefreshClaimByIdAsync<T>(this UserManager<T> userManager,
            string userId,
            ResponseUserModel responseUserModel, IServiceProvider serviceProvider)
            where T : class
        {
            return await userManager.RefreshClaimByUserAsync(
                await userManager.FindByIdAsync(userId),
                responseUserModel,
                serviceProvider);
        }

        public static async Task<IdentityResult> RefreshClaimByEmailAsync<T>(this UserManager<T> userManager,
            string email,
            ResponseUserModel responseUserModel, IServiceProvider serviceProvider)
            where T : class
        {
            return await userManager.RefreshClaimByUserAsync(
                await userManager.FindByEmailAsync(email),
                responseUserModel,
                serviceProvider);
        }

        public static async Task<IdentityResult> RefreshClaimByUserAsync<T>(this UserManager<T> userManager,
            T user, ResponseUserModel responseUserModel, IServiceProvider serviceProvider)
            where T : class
        {
            if (user == default)
                return IdentityResult.Failed();
            var simpleAuthClaim = await responseUserModel.GenerateSimpleAuthClaimAsync(serviceProvider);
            if (simpleAuthClaim == default)
                return IdentityResult.Failed();
            var existingClaims = await userManager.GetClaimsAsync(user);
            var existingSimpleAuthClaims = existingClaims.Where(x => x.Type == simpleAuthClaim.Type).ToList();
            if (existingSimpleAuthClaims.Any())
                await userManager.RemoveClaimsAsync(user, existingSimpleAuthClaims);
            return await userManager.AddClaimAsync(user, simpleAuthClaim);
        }
    }
}