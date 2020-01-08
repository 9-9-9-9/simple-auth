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
using SimpleAuth.Shared;
using SimpleAuth.Shared.Utils;

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
            try
            {
                var endpoint = GetEndpoint(httpContext);

                var saP = endpoint?.Metadata.OfType<SaPermissionAttribute>().OrEmpty().ToList();
                var saM = endpoint?.Metadata.GetMetadata<SaModuleAttribute>();
                if (saP.IsAny())
                {
                    if (saM != null)
                    {
                        var claim =
                            httpContext.User.Claims.FirstOrDefault(x => x.Type == nameof(SimpleAuthorizationClaims));
                        if (claim == null)
                        {
                            await httpContext.Response
                                .WithStatus(StatusCodes.Status403Forbidden)
                                .WithBody($"Missing {nameof(SimpleAuthorizationClaims)}");
                            return;
                        }

                        var jsonService = httpContext.RequestServices.GetService<IJsonService>();
                        var simpleAuthorizationClaims = jsonService.Deserialize<SimpleAuthorizationClaims>(claim.Value);
                        
                        if (simpleAuthorizationClaims.Claims.IsEmpty())
                        {
                            await httpContext.Response
                                .WithStatus(StatusCodes.Status403Forbidden)
                                .WithBody($"Missing {nameof(SimpleAuthorizationClaims.Claims)}");
                            return;
                        }

                        var requireTenant = httpContext.RequestServices
                            .GetService<ITenantProvider>()
                            .GetTenant(httpContext);

                        foreach (var permissionAttribute in saP)
                        {
                            var roleFromModule =
                                RoleUtils.JoinPartsFromModule(saM.Module, permissionAttribute.SubModules);

                            var existingRole = simpleAuthorizationClaims.Claims.FirstOrDefault(x =>
                                x.Module == roleFromModule
                                &&
                                (
                                    x.Tenant == Constants.WildCard
                                    ||
                                    x.Tenant == requireTenant
                                )
                            );
                            if (existingRole == default)
                            {
                                await httpContext.Response
                                    .WithStatus(StatusCodes.Status403Forbidden)
                                    .WithBody($"Require tenant '{requireTenant}', module '{roleFromModule}'");
                                return;
                            }

                            if (existingRole.PermissionEnum.HasFlag(permissionAttribute.Permission))
                            {
                                // pass
                            }
                            else
                            {
                                await httpContext.Response
                                    .WithStatus(StatusCodes.Status403Forbidden)
                                    .WithBody($"{roleFromModule}, require {permissionAttribute.Permission}");
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
            catch
            {
                // TODO remove
                await _next(httpContext);
            }
        }

        private static Endpoint GetEndpoint(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Features.Get<IEndpointFeature>()?.Endpoint;
        }
    }
}