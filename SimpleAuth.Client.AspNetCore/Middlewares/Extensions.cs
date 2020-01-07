using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Middlewares;
using SimpleAuth.Client.AspNetCore.Services;

namespace Microsoft.AspNetCore.Builder
{
    public static class Extensions
    {
        public static IApplicationBuilder UseSaAuthorization(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SaAuthorizationMiddleware>();
        }

        public static HttpResponse WithStatus(this HttpResponse httpResponse, int status)
        {
            httpResponse.StatusCode = status;
            return httpResponse;
        }

        public static HttpResponse WithStatus(this HttpResponse httpResponse, HttpStatusCode status)
        {
            httpResponse.StatusCode = (int) status;
            return httpResponse;
        }

        public static async Task<HttpResponse> WithBody(this HttpResponse httpResponse, string content)
        {
            return await httpResponse.WithBody(Encoding.UTF8.GetBytes(content), "text/plain; charset=UTF-8");
        }

        public static async Task<HttpResponse> WithBody(this HttpResponse httpResponse, byte[] buffer, string contentType)
        {
            httpResponse.ContentType = contentType;
            await httpResponse.Body.WriteAsync(buffer);
            return httpResponse;
        }

        public static IServiceCollection UseTenantProvider<TTenantProvider>(this IServiceCollection serviceCollection)
            where TTenantProvider : class, ITenantProvider
        {
            serviceCollection.AddTransient<ITenantProvider, TTenantProvider>();
            return serviceCollection;
        }
    }
}