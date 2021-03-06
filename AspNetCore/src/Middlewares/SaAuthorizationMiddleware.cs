using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Client.AspNetCore.Models;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SaAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public SaAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var authenticationInfoProvider = httpContext.RequestServices.GetService<IAuthenticationInfoProvider>();
            var endpoint = GetEndpoint(httpContext);

            var saP = endpoint?.Metadata.OfType<SaPermissionAttribute>().OrEmpty().ToList();
            var saM = endpoint?.Metadata.GetMetadata<SaModuleAttribute>();

            if (
                (saP.IsAny() || saM != null)
                &&
                !await authenticationInfoProvider.IsAuthenticated(httpContext)
            )
            {
                httpContext.Response
                    .WithStatus(StatusCodes.Status403Forbidden);
                return;
            }

            if (saP.IsAny())
            {
                if (saM != null)
                {
                    var configurationProvider =
                        httpContext.RequestServices.GetService<ISimpleAuthConfigurationProvider>();
                    var tenantProvider = httpContext.RequestServices.GetService<ITenantProvider>();
                    var requireTenant = await tenantProvider.GetTenantAsync(httpContext);

                    var requireClaims = new ClaimsBuilder()
                        .WithModule(saM)
                        .WithPermissions(saP)
                        .Build(configurationProvider, requireTenant)
                        .ToArray();

                    try
                    {
                        await authenticationInfoProvider.AuthorizeAsync(httpContext, requireClaims);
                    }
                    catch (DataVerificationMismatchException ex)
                    {
                        await httpContext.Response
                            .WithStatus(StatusCodes.Status403Forbidden)
                            .WithBody(ex.Message);
                        return;
                    }
                }
            }
            else
            {
                if (saM != null)
                {
                    if (saM.Restricted)
                    {
                        await httpContext.Response
                            .WithStatus(StatusCodes.Status403Forbidden)
                            .WithBody(
                                $"{nameof(SaModuleAttribute)}(module={saM.Module}) is restricted, require {nameof(SaPermissionAttribute)} pre-defined at Action"
                            );
                        return;
                    }
                    else
                    {
                        // pass
                    }
                }
                else
                {
                    // pass
                }
            }
            await _next(httpContext);
        }

        private static Endpoint GetEndpoint(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Features.Get<IEndpointFeature>()?.Endpoint;
        }
    }
}