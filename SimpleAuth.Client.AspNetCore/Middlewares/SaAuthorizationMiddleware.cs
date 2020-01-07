using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

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
            try
            {
                var endpoint = GetEndpoint(httpContext);
                var saM = endpoint?.Metadata.GetMetadata<SaModuleAttribute>();
                if (saM != null)
                {
                    // ReSharper disable once JoinDeclarationAndInitializer
                    // ReSharper disable once CollectionNeverUpdated.Local
                    List<ModuleClaim> moduleClaims;
#if DEBUG
                    moduleClaims = new List<ModuleClaim>();
#else
                    if (!httpContext.User.Identity.IsAuthenticated)
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }

                    moduleClaims = httpContext.User.Claims.OfType<ModuleClaim>().ToList();
                    if (!moduleClaims.IsAny())
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }
#endif

                    var saP = endpoint.Metadata.OfType<SaPermissionAttribute>().OrEmpty().ToList();
                    if (saP.IsAny())
                    {
                        foreach (var permissionAttribute in saP)
                        {
                            var roleFromModule =
                                RoleUtils.JoinPartsFromModule(saM.Module, permissionAttribute.SubModules);

                            var existingRole = moduleClaims.FirstOrDefault(x => x.Type == roleFromModule);
                            if (existingRole == default)
                            {
                                await httpContext.Response
                                    .WithStatus(StatusCodes.Status403Forbidden)
                                    .WithBody(roleFromModule);
                                return;
                            }

                            if (existingRole.Permission.HasFlag(permissionAttribute.Permission))
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