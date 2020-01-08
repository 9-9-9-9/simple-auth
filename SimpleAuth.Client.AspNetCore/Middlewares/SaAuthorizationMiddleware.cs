using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Models;
using SimpleAuth.Core.Extensions;

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
                    var claims = await authenticationInfoProvider.GetClaims(httpContext);

                    var tenantProvider = httpContext.RequestServices.GetService<ITenantProvider>();
                    var requireTenant = await tenantProvider.GetTenantAsync(httpContext);

                    var requireClaims = saP.Select(x => new SimpleAuthorizationClaim(
                        requireTenant,
                        saM.Module,
                        x.SubModules,
                        x.Permission
                    ));

                    var missingClaims = (await claims.GetMissingClaimsAsync(requireClaims, httpContext.RequestServices)).OrEmpty().ToArray();
                    if (missingClaims.Any())
                    {
                        await httpContext.Response
                            .WithStatus(StatusCodes.Status403Forbidden)
                            .WithBody(
                                $"Require tenant '{missingClaims[0].Tenant}', module '{missingClaims[0].Module}', sub modules [{string.Join(",", missingClaims[0].SubModules)}], permission {missingClaims[0].Permission}");
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