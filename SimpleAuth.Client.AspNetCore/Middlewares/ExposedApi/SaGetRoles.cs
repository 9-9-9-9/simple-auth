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
            var userClaims = httpContext.GetUserSimpleAuthorizationClaimsFromContext();
            
            await httpContext.Response
                .WithStatus(StatusCodes.Status200OK)
                .WithBody(
                    JsonConvert.SerializeObject(userClaims.Select(c => RoleUtils.ComputeRoleId(c.ClientRoleModel.Corp, c.ClientRoleModel.App, c.ClientRoleModel.Env, c.ClientRoleModel.Tenant, c.ClientRoleModel.Module, c.ClientRoleModel.SubModules) + $":{c.ClientRoleModel.Permission}").ToArray())
                );
        }
    }
}