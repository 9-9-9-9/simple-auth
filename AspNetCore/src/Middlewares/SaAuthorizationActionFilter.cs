using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Client.AspNetCore.Models;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Shared.Exceptions;

public class SaAuthorizationAsyncActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        var methodInfoOfAction = context.ActionDescriptor.GetType().GetProperty("MethodInfo")
            ?.GetValue(context.ActionDescriptor) as MethodInfo;
        if (methodInfoOfAction == null)
        {
            httpContext.Response
                .WithStatus(StatusCodes.Status403Forbidden);
            return;
        }

        var controller = context.Controller;

        var authenticationInfoProvider = httpContext.RequestServices.GetService<IAuthenticationInfoProvider>();

        var saP = methodInfoOfAction.GetCustomAttributes<SaPermissionAttribute>(false).OrEmpty().ToList();
        var saM = methodInfoOfAction.GetCustomAttribute<SaModuleAttribute>(false) ?? controller.GetType().GetCustomAttribute<SaModuleAttribute>();

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

        await next();
    }
}