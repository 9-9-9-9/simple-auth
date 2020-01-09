using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Models;
using SimpleAuth.Core.Extensions;

namespace Microsoft.AspNetCore.Http
{
    public static class AspNetCoreHttpAdditionalExtensions
    {
        internal static HttpResponse WithStatus(this HttpResponse httpResponse, int status)
        {
            httpResponse.StatusCode = status;
            return httpResponse;
        }

        internal static async Task WithBody(this HttpResponse httpResponse, string content)
        {
            await httpResponse.WithBody(Encoding.UTF8.GetBytes(content), "text/plain; charset=UTF-8");
        }

        private static async Task WithBody(this HttpResponse httpResponse, byte[] buffer, string contentType)
        {
            httpResponse.ContentType = contentType;
            await httpResponse.Body.WriteAsync(buffer);
        }

        public static async Task<IEnumerable<SimpleAuthorizationClaim>> GetMissingClaimsAsync(
            this IEnumerable<Claim> claims, IEnumerable<SimpleAuthorizationClaim> requiredClaims, IServiceProvider serviceProvider)
        {
            var authenticationInfoProvider = serviceProvider.GetService<IAuthenticationInfoProvider>();
            var saClaim = authenticationInfoProvider.GetSimpleAuthClaim(claims);
            if (saClaim == default)
                return requiredClaims; // missing all
            
            var simpleAuthorizationClaims = (await authenticationInfoProvider.GetSimpleAuthClaimsAsync(saClaim)).OrEmpty().ToArray();
            if (simpleAuthorizationClaims.Length == 0)
                return requiredClaims; // missing all

            return requiredClaims.Where(x => !simpleAuthorizationClaims.Any(y => y.Contains(x)));
        }
    }
}