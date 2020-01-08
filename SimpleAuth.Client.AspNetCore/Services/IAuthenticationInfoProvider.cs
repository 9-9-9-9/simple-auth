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
        public async Task<bool> IsAuthenticated(HttpContext httpContext)
        {
            await Task.CompletedTask;
            return httpContext.User.Identity.IsAuthenticated;
        }

        public async Task<IEnumerable<Claim>> GetClaims(HttpContext httpContext)
        {
            await Task.CompletedTask;
            return httpContext.User.Claims;
        }
    }
}