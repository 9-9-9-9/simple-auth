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
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleGroup(string name)
        {
            return await ProcedureResponseForLookUp(() =>
                FindRoleGroupAsync(name, RequestAppHeaders.Corp, RequestAppHeaders.App)
            );
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        [HttpPost("{groupName}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LockRoleGroup(string groupName)
        {
            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatusAsync(new Shared.Domains.RoleGroup
                    {
                        Name = groupName,
                        Corp = RequestAppHeaders.Corp,
                        App = RequestAppHeaders.App,
                        Locked = true
                    });
                }
            );
        }

        [HttpDelete("{groupName}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlockRoleGroup(string groupName)
        {
            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatusAsync(new Shared.Domains.RoleGroup
                    {
                        Name = groupName,
                        Corp = RequestAppHeaders.Corp,
                        App = RequestAppHeaders.App,
                        Locked = false
                    });
                }
            );
        }


        [HttpGet("{groupName}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [HttpDelete, Route("{groupName}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRoles(
            string groupName,
            [FromQuery] string[] roles,
            [FromQuery] bool all)
        {
            if (all && roles.IsAny())
                return BadRequest();

            if (!all && !roles.IsAny())
                return BadRequest();

            return await ProcedureDefaultResponse(async () =>
            {
                var group = new Shared.Domains.RoleGroup
                {
                    Name = groupName,
                    Corp = RequestAppHeaders.Corp,
                    App = RequestAppHeaders.App
                };
                if (all)
                    await Service.DeleteAllRolesFromGroupAsync(group);
                else
                    await Service.DeleteRolesFromGroupAsync(group, roles.Select(RoleUtils.UnMerge)
                        .Select(tp => new RoleModel
                        {
                            Role = tp.Item1,
                            Permission = tp.Item2.Serialize()
                        })
                        .ToArray());
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