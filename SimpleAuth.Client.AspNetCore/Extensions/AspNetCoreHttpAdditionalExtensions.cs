using System;
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

        public static async Task<PackageSimpleAuthorizationClaim> GetUserPackageSimpleAuthorizationClaimAsync(
            this HttpContext httpContext)
        {
            var authenticationInfoProvider = httpContext.RequestServices.GetService<IAuthenticationInfoProvider>();
            var claims = await authenticationInfoProvider.GetClaimsAsync(httpContext);
            var simpleAuthClaim = authenticationInfoProvider.GetSimpleAuthClaim(claims);
            if (simpleAuthClaim == default)
                return default;
            return await authenticationInfoProvider.GetPackageSimpleAuthClaimAsync(simpleAuthClaim);
        }

        public static PackageSimpleAuthorizationClaim GetUserPackageSimpleAuthorizationClaimFromContext(
            this HttpContext httpContext)
        {
            return httpContext.GetItemOrDefault<PackageSimpleAuthorizationClaim>();
        }

        public static void AddUserPackageSimpleAuthorizationClaimIntoContext(
            this HttpContext httpContext, PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim)
        {
            httpContext.PushItem(packageSimpleAuthorizationClaim);
        }

        public static async Task<ICollection<SimpleAuthorizationClaim>> GetMissingClaimsAsync(
            this HttpContext httpContext,
            IEnumerable<SimpleAuthorizationClaim> requiredClaims)
        {
            var existingClaims = (await httpContext.GetUserPackageSimpleAuthorizationClaimAsync()).ClaimsOrEmpty;
            return existingClaims.GetMissingClaims(requiredClaims);
        }

        public static async Task<ICollection<SimpleAuthorizationClaim>> GetMissingClaimsAsync(
            this HttpContext httpContext,
            ClaimsBuilder claimsBuilder)
        {
            var existingClaims = (await httpContext.GetUserPackageSimpleAuthorizationClaimAsync()).ClaimsOrEmpty;
            return existingClaims.GetMissingClaims(claimsBuilder.Build(httpContext));
        }

        public static HttpContext PushItem<T>(this HttpContext httpContext, T item, string key = null,
            bool skipIfExists = false)
        {
            if (item == null)
                return httpContext;

            if (key == null)
                key = typeof(T).FullName;

            // ReSharper disable once AssignNullToNotNullAttribute
            if (!httpContext.Items.ContainsKey(key))
            {
                httpContext.Items.Add(key, item);
                return httpContext;
            }

            if (skipIfExists)
                return httpContext;

            httpContext.Items[key] = item;
            return httpContext;
        }

        public static T GetItemOrDefault<T>(this HttpContext httpContext, string key = null)
        {
            if (key == null)
                key = typeof(T).FullName;

            // ReSharper disable once AssignNullToNotNullAttribute
            if (!httpContext.Items.ContainsKey(key))
                return default;

            var result = httpContext.Items[key];
            if (result == default)
                return default;

            if (result is T tObject)
                return tObject;

            throw new InvalidCastException(
                $"Key '{key}' is data of type {result.GetType()}, can not be cast to {typeof(T)}");
        }
    }
}