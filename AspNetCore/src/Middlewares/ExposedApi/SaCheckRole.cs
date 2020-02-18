using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SimpleAuth.Client.AspNetCore.Models;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SaCheckRole
    {
        private const string ParamEnv = "env";
        private const string ParamTenant = "tenant";
        private const string ParamModule = "module";
        private const string ParamSubModules = "subModules";
        private const string ParamPermission = "permission";

        // ReSharper disable once NotAccessedField.Local
        private readonly RequestDelegate _next;

        public SaCheckRole(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var userClaims = httpContext.GetUserPackageSimpleAuthorizationClaimFromContext().ClaimsOrEmpty;

            if (!userClaims.IsAny())
            {
                httpContext.Response.WithStatus(StatusCodes.Status406NotAcceptable);
                return;
            }

            // ReSharper disable JoinDeclarationAndInitializer
            string env, tenant, module, subModules, permissions;
            // ReSharper restore JoinDeclarationAndInitializer

            if (!TryExtractQueryParameter(httpContext.Request.Query, ParamEnv, out var envs, false, false))
            {
                await ResponseBadParameterAsync(ParamEnv, httpContext);
                return;
            }

            env = envs.FirstOrDefault();

            if (!TryExtractQueryParameter(httpContext.Request.Query, ParamTenant, out var tenants, false, false))
            {
                await ResponseBadParameterAsync(ParamTenant, httpContext);
                return;
            }

            tenant = tenants.FirstOrDefault();

            if (!TryExtractQueryParameter(httpContext.Request.Query, ParamModule, out var modules, true, false))
            {
                await ResponseBadParameterAsync(ParamModule, httpContext);
                return;
            }

            module = modules.Single();

            if (!TryExtractQueryParameter(httpContext.Request.Query, ParamSubModules, out var qSubModules, false,
                false))
            {
                await ResponseBadParameterAsync(ParamSubModules, httpContext);
                return;
            }

            subModules = qSubModules.FirstOrDefault();
            var strSubPermissions = subModules?.Split(Constants.SplitterSubModules) ?? new string[0];

            if (strSubPermissions.Any(x => x.IsBlank()))
            {
                await ResponseBadParameterAsync(ParamSubModules, httpContext);
                return;
            }

            if (!TryExtractQueryParameter(httpContext.Request.Query, ParamPermission, out var qPermissions, true,
                false))
            {
                await ResponseBadParameterAsync(ParamPermission, httpContext);
                return;
            }

            permissions = qPermissions.Single();

            var permission = Verb.None;
            try
            {
                var ePermissions = permissions.Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .Select(x => (Verb) Enum.Parse(typeof(Verb), x, true))
                    .ToList();

                if (ePermissions.IsEmpty())
                {
                    await ResponseBadParameterAsync(ParamPermission, httpContext);
                    return;
                }

                ePermissions.ForEach(x => permission |= x);
            }
            catch
            {
                await ResponseBadParameterAsync(ParamPermission, httpContext);
                return;
            }

            var claimsBuilder = new ClaimsBuilder().WithModule(module).WithPermission(permission, strSubPermissions);

            var simpleAuthConfigurationProvider = httpContext.RequestServices.GetService<ISimpleAuthConfigurationProvider>();
            var tenantProvider = httpContext.RequestServices.GetService<ITenantProvider>();
            var requiredClaims = claimsBuilder.Build(simpleAuthConfigurationProvider.Corp,
                simpleAuthConfigurationProvider.App, env ?? simpleAuthConfigurationProvider.Env,
                tenant ?? tenantProvider.GetTenant(httpContext));

            var missingClaims = await httpContext.GetMissingClaimsAsync(requiredClaims);

            if (missingClaims.IsAny())
            {
                await httpContext.Response
                    .WithStatus(StatusCodes.Status406NotAcceptable)
                    .WithBody(
                        JsonConvert.SerializeObject(missingClaims.Select(c =>
                            RoleUtils.ComputeRoleId(c.ClientPermissionModel.Corp, c.ClientPermissionModel.App,
                                c.ClientPermissionModel.Env, c.ClientPermissionModel.Tenant, c.ClientPermissionModel.Module,
                                c.ClientPermissionModel.SubModules) + $":{c.ClientPermissionModel.Verb}").ToArray())
                    );
                return;
            }

            httpContext.Response.WithStatus(StatusCodes.Status200OK);
        }

        private bool TryExtractQueryParameter(IQueryCollection queryCollection, string paramName,
            out StringValues values, bool require, bool allowMultiple)
        {
            if (queryCollection.TryGetValue(paramName, out values))
            {
                values = new StringValues(values.Select(x => x?.Trim()).Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray());
                if (!allowMultiple && values.Count > 1)
                    return false;
                return true;
            }
            else
            {
                values = default;
                return !require;
            }
        }

        private Task ResponseBadParameterAsync(string parameterName, HttpContext httpContext)
        {
            return httpContext.Response.WithStatus(StatusCodes.Status400BadRequest)
                .WithBody($"Bad parameter '{parameterName}'");
        }
    }
}