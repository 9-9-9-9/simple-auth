using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SaGetRoles
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly RequestDelegate _next;

        public SaGetRoles(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var userClaims = httpContext.GetUserPackageSimpleAuthorizationClaimFromContext().ClaimsOrEmpty;
            
            await httpContext.Response
                .WithStatus(StatusCodes.Status200OK)
                .WithBody(
                    JsonConvert.SerializeObject(userClaims.Select(c => RoleUtils.ComputeRoleId(c.ClientPermissionModel.Corp, c.ClientPermissionModel.App, c.ClientPermissionModel.Env, c.ClientPermissionModel.Tenant, c.ClientPermissionModel.Module, c.ClientPermissionModel.SubModules) + $":{c.ClientPermissionModel.Verb}").ToArray())
                );
        }
    }
}