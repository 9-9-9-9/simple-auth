using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;

namespace SimpleAuth.Server.Controllers
{
    [Route("api/role-groups")]
    [RequireAppToken]
    public class RoleGroupsController : BaseController<IRoleGroupService, IRoleGroupRepository, RoleGroup>
    {
        private readonly IRoleGroupValidationService _roleGroupValidation;

        public RoleGroupsController(IServiceProvider serviceProvider,
            IRoleGroupValidationService roleGroupValidation) :
            base(serviceProvider)
        {
            _roleGroupValidation = roleGroupValidation;
        }

        [HttpPost]
        public async Task<IActionResult> AddRoleGroup([FromBody] CreateRoleGroupModel model)
        {
            if (model.Name.IsBlank())
                model.Name = $"{model.Corp}-{model.App}-{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8)}"
                    .NormalizeInput();

            if (!ModelState.IsValid)
                return BadRequest(nameof(ModelState));

            if (model.Corp != RequestAppHeaders.Corp || model.App != RequestAppHeaders.App)
                return CrossAppToken();

            var vr = _roleGroupValidation.IsValid(model);
            if (!vr.IsValid)
                return BadRequest(vr.Message);

            return await ProcedureResponseForPersistAction(async () =>
                await Service.AddRoleGroupAsync(model)
            );
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetRoleGroup(string name)
        {
            return await ProcedureResponseForLookUp(() =>
                FindRoleGroupAsync(name, RequestAppHeaders.Corp, RequestAppHeaders.App)
            );
        }

        [HttpGet]
        public async Task<IActionResult> FindRoleGroups(
            [FromQuery] string term,
            [FromQuery] int? skip,
            [FromQuery] int? take)
        {
            return await ProcedureResponseForLookUpArrayUsingTerm(term, skip, take,
                findOptions =>
                    Task.FromResult(
                        Service
                            .SearchRoleGroups(term, RequestAppHeaders.Corp, RequestAppHeaders.App, findOptions)
                            .Select(x => x.Name)
                    )
            );
        }

        [HttpPost, HttpPut, Route("{name}/lock")]
        public async Task<IActionResult> UpdateLock(string name)
        {
            var @lock = !Request.Method.EqualsIgnoreCase(HttpMethods.Delete);

            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatusAsync(new Shared.Domains.RoleGroup
                    {
                        Name = name,
                        Corp = RequestAppHeaders.Corp,
                        App = RequestAppHeaders.App,
                        Locked = @lock
                    });
                }
            );
        }


        [HttpGet("{groupName}/roles")]
        public async Task<IActionResult> GetRoles(string groupName)
        {
            return await ProcedureResponseForArrayLookUp(() =>
                FindRoleGroupAsync(groupName, RequestAppHeaders.Corp, RequestAppHeaders.App)
                    .ContinueWith(x =>
                        x.Result.Roles.OrEmpty().Select(RoleModel.Cast)
                    )
            );
        }

        [HttpPost, HttpPut, Route("{groupName}/roles")]
        public async Task<IActionResult> UpdateRoles(
            string groupName,
            [FromBody] UpdateRolesModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(nameof(ModelState));

            return await ProcedureDefaultResponse(async () =>
            {
                var group = await FindRoleGroupAsync(groupName, RequestAppHeaders.Corp, RequestAppHeaders.App);
                await Service.AddRolesToGroupAsync(group, model.Roles);
            });
        }

        [HttpDelete, Route("{name}/roles")]
        public async Task<IActionResult> DeleteRoles(
            string name,
            [FromBody] DeleteRolesModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(nameof(ModelState));

            return await ProcedureDefaultResponse(async () =>
            {
                var group = await FindRoleGroupAsync(name, RequestAppHeaders.Corp, RequestAppHeaders.App);
                if (model.Roles.IsAny())
                    await Service.DeleteRolesFromGroupAsync(group, model.Roles);
                else
                    await Service.DeleteAllRolesFromGroupAsync(group);
            });
        }

        private async Task<Shared.Domains.RoleGroup> FindRoleGroupAsync(string name, string corp, string app)
        {
            var group = await Service.GetRoleGroupByName(name, corp, app);
            if (group == null)
                throw new EntityNotExistsException(name);
            return group;
        }
    }
}