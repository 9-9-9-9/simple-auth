using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Services;

namespace SimpleAuth.Client.Models
{
    public static class SimpleAuthorizationClaimAdditionalExtensions
    {
        public static async Task<Claim> GenerateSimpleAuthClaimAsync(this IEnumerable<SimpleAuthorizationClaim> claims,
            IServiceProvider serviceProvider)
        {
            var authenticationInfoProvider = serviceProvider.GetService<IAuthenticationInfoProvider>();
            return await authenticationInfoProvider.GenerateSimpleAuthClaimAsync(claims);
        }
    }
}