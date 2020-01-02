using SimpleAuth.Client.AspNetCore.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class Extensions
    {
        public static IApplicationBuilder UseSaAuthorization(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SaAuthorizationMiddleware>();
        }
    }
}