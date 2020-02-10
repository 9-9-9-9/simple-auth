using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Client.AspNetCore.Models;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Models;
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

                    var packageSimpleAuthorizationClaim = httpContext.GetUserPackageSimpleAuthorizationClaimFromContext();

                    if (configurationProvider.LiveChecking)
                    {
                        if (!await PerformLiveCheckingPermission(httpContext, packageSimpleAuthorizationClaim, requireClaims)) 
                            return;
                    }
                    else
                    {
                        if (!await PerformLocalCheckingPermission(httpContext, packageSimpleAuthorizationClaim, requireClaims)) 
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

        private static async Task<bool> PerformLocalCheckingPermission(HttpContext httpContext,
            PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim, SimpleAuthorizationClaim[] requireClaims)
        {
            var userClaims = packageSimpleAuthorizationClaim.ClaimsOrEmpty;

            if (!userClaims.IsAny())
            {
                await httpContext.Response
                    .WithStatus(StatusCodes.Status403Forbidden)
                    .WithBody(
                        "User doesn't have any permission'"
                    );
                return false;
            }

            var missingClaims = userClaims.GetMissingClaims(requireClaims)
                .OrEmpty()
                .ToArray();

            if (missingClaims.Any())
            {
                await httpContext.Response
                    .WithStatus(StatusCodes.Status403Forbidden)
                    .WithBody(
                        $"Require {missingClaims[0].ClientRoleModel}"
                    );
                return false;
            }

            return true;
        }

        private static async Task<bool> PerformLiveCheckingPermission(HttpContext httpContext,
            PackageSimpleAuthorizationClaim packageSimpleAuthorizationClaim, SimpleAuthorizationClaim[] requireClaims)
        {
            if (packageSimpleAuthorizationClaim.UserId.IsBlank())
            {
                await httpContext.Response
                    .WithStatus(StatusCodes.Status403Forbidden)
                    .WithBody(
                        $"Can't find user id from {nameof(PackageSimpleAuthorizationClaim)}"
                    );
                return false;
            }
            
            var userAuthService = httpContext.RequestServices.GetService<IUserAuthService>();
            var missingRoles = await userAuthService.GetMissingRolesAsync(packageSimpleAuthorizationClaim.UserId, new RoleModels
            {
                Roles = requireClaims.Select(x => x.ClientRoleModel.ToRole().Cast()).ToArray()
            });

            if (missingRoles.Any())
            {
                var firstRoleModel = missingRoles.First();
                RoleUtils.Parse(firstRoleModel.Role, firstRoleModel.Permission, out var clientRoleModel);
                await httpContext.Response
                    .WithStatus(StatusCodes.Status403Forbidden)
                    .WithBody(
                        $"Require {clientRoleModel}"
                    );
                return false;
            }

            return true;
        }

        private static Endpoint GetEndpoint(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Features.Get<IEndpointFeature>()?.Endpoint;
        }
    }
}