using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public interface IAuthenticationInfoProvider
    {
        Task<bool> IsAuthenticated(HttpContext httpContext);
        Task<IEnumerable<Claim>> GetClaims(HttpContext httpContext);
    }

    public class DefaultAuthenticationInfoProvider : IAuthenticationInfoProvider
    {
        public Task<bool> IsAuthenticated(HttpContext httpContext)
        {
            return Task.FromResult(httpContext.User.Identity.IsAuthenticated);
        }

        public Task<IEnumerable<Claim>> GetClaims(HttpContext httpContext)
        {
            return Task.FromResult(httpContext.User.Claims);
        }
    }
}