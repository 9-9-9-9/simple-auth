using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
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
            var endpoint = GetEndpoint(httpContext);

            var saP = endpoint?.Metadata.OfType<SaPermissionAttribute>().OrEmpty().ToList();
            var saM = endpoint?.Metadata.GetMetadata<SaModuleAttribute>();
            if (saP.IsAny())
            {
                if (saM != null)
                {
                    var claim = httpContext.User.Claims.OfSimpleAuth();
                    if (claim == default)
                    {
                        await httpContext.Response
                            .WithStatus(StatusCodes.Status403Forbidden)
                            .WithBody($"Missing {SimpleAuthDefaults.ClaimType}");
                        return;
                    }

                    var jsonService = httpContext.RequestServices.GetService<IJsonService>();
                    var simpleAuthorizationClaims = jsonService.Deserialize<SimpleAuthorizationClaim[]>(claim.Value);

                    if (simpleAuthorizationClaims.IsEmpty())
                    {
                        await httpContext.Response
                            .WithStatus(StatusCodes.Status403Forbidden)
                            .WithBody($"Missing {nameof(SimpleAuthDefaults.ClaimType)}");
                        return;
                    }

                    var requireTenant = httpContext.RequestServices
                        .GetService<ITenantProvider>()
                        .GetTenant(httpContext);

                    foreach (var permissionAttribute in saP)
                    {
                        var requiredClaim = new SimpleAuthorizationClaim(
                            requireTenant,
                            saM.Module,
                            permissionAttribute.SubModules,
                            permissionAttribute.Permission
                        );

                        var hasPermission = simpleAuthorizationClaims.Any(x =>
                            x.Contains(requiredClaim)
                        );

                        if (!hasPermission)
                        {
                            await httpContext.Response
                                .WithStatus(StatusCodes.Status403Forbidden)
                                .WithBody(
                                    $"Require tenant '{requiredClaim.Tenant}', module '{requiredClaim.Module}', sub modules [{string.Join(",", requiredClaim.SubModules)}], permission {requiredClaim.Permission}");
                            return;
                        }
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