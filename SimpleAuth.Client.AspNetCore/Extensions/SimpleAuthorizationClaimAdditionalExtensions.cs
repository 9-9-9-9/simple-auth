using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Models
{
    public static class SimpleAuthorizationClaimAdditionalExtensions
    {
        public static async Task<Claim> GenerateSimpleAuthClaimAsync(this ICollection<SimpleAuthorizationClaim> claims,
            IServiceProvider serviceProvider)
        {
            var authenticationInfoProvider = serviceProvider.GetService<IAuthenticationInfoProvider>();
            return await authenticationInfoProvider.GenerateSimpleAuthClaimAsync(claims);
        }
        
        public static Task<Claim> GenerateSimpleAuthClaimAsync(this ICollection<RoleModel> roleModels,
            IServiceProvider serviceProvider)
        {
            return roleModels.ToSimpleAuthorizationClaims().GenerateSimpleAuthClaimAsync(serviceProvider);
        }
        
        public static Task<Claim> GenerateSimpleAuthClaimAsync(this ResponseUserModel responseUserModel,
            IServiceProvider serviceProvider)
        {
            return responseUserModel?.ActiveRoles.ToSimpleAuthorizationClaims().GenerateSimpleAuthClaimAsync(serviceProvider);
        }
    }
}