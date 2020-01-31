using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Services;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SaPushClaimsToContextMiddleware
    {
        private readonly RequestDelegate _next;

        public SaPushClaimsToContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var authenticationInfoProvider = httpContext.RequestServices.GetService<IAuthenticationInfoProvider>();

            if (await authenticationInfoProvider.IsAuthenticated(httpContext))
            {
                var packageSimpleAuthorizationClaim = await httpContext.GetUserPackageSimpleAuthorizationClaimAsync();
                httpContext.AddUserSimpleAuthorizationClaimsIntoContext(packageSimpleAuthorizationClaim.ClaimsOrEmpty);
            }

            await _next(httpContext);
        }
    }
}