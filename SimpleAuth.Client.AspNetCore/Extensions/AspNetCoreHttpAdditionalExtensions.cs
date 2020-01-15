using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Models;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Utils;

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

        public static ICollection<SimpleAuthorizationClaim> GetMissingClaims(
            this IEnumerable<SimpleAuthorizationClaim> existingClaims,
            IEnumerable<SimpleAuthorizationClaim> requiredClaims)
        {
            return AuthorizationUtils.GetMissingClaims(existingClaims, requiredClaims);
        }

        public static async Task<ICollection<SimpleAuthorizationClaim>> GetUserSimpleAuthorizationClaims(
            this HttpContext httpContext)
        {
            var authenticationInfoProvider = httpContext.RequestServices.GetService<IAuthenticationInfoProvider>();
            var claims = await authenticationInfoProvider.GetClaimsAsync(httpContext);
            var simpleAuthClaim = authenticationInfoProvider.GetSimpleAuthClaim(claims);
            return await authenticationInfoProvider.GetSimpleAuthClaimsAsync(simpleAuthClaim);
        }
        
        public static async Task<ICollection<SimpleAuthorizationClaim>> GetMissingClaimsAsync(
            this HttpContext httpContext,
            IEnumerable<SimpleAuthorizationClaim> requiredClaims)
        {
            return (await httpContext.GetUserSimpleAuthorizationClaims()).GetMissingClaims(requiredClaims);
        }
        
        public static async Task<ICollection<SimpleAuthorizationClaim>> GetMissingClaimsAsync(
            this HttpContext httpContext,
            ClaimsBuilder claimsBuilder)
        {
            return (await httpContext.GetUserSimpleAuthorizationClaims()).GetMissingClaims(claimsBuilder.Build(httpContext));
        }
    }
}