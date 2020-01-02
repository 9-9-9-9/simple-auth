using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using SimpleAuth.Core.Extensions;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    public class SaAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public SaAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var endpoint = GetEndpoint(httpContext);

                if (endpoint != null)
                {
                    var saM = endpoint.Metadata.GetMetadata<SaModuleAttribute>();
                    var saP = endpoint.Metadata.GetOrderedMetadata<SaPermissionAttribute>();

                    if (saP.IsAny())
                    {
                        if (saM == null)
                            throw new InvalidOperationException($"Can not find declaration of {nameof(SaModuleAttribute)}");
                        
                    }
                }

                await _next(httpContext);
            }
            else
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        }

        private static Endpoint GetEndpoint(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Features.Get<IEndpointFeature>()?.Endpoint;
        }
    }
}