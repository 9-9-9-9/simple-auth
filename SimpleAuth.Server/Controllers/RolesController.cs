using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;

namespace SimpleAuth.Server.Controllers
{
    [Route("api/roles")]
    [RequireAppToken]
    public class RolesController : BaseController<IRoleService, IRoleRepository, Role>
    {
        private readonly IRoleValidationService _roleValidationService;

        public RolesController(IServiceProvider serviceProvider, IRoleValidationService roleValidationService) : base(
            serviceProvider)
        {
            _roleValidationService = roleValidationService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddRole([FromBody] CreateRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(nameof(ModelState));

            if (model.Corp != RequestAppHeaders.Corp || model.App != RequestAppHeaders.App)
                return CrossAppToken();

            var vr = _roleValidationService.IsValid(model);
            if (!vr.IsValid)
                return BadRequest(vr.Message);

            return await ProcedureResponseForPersistAction(async () =>
                await Service.AddRoleAsync(model)
            );
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindRoles(
            [FromQuery, Required] string term,
            [FromQuery] int skip,
            [FromQuery] int take
        )
        {
            return await ProcedureResponseForLookUpArrayUsingTerm(term, skip, take,
                findOptions =>
                {
                    var res = Service
                        .SearchRoles(term, RequestAppHeaders.Corp, RequestAppHeaders.App, findOptions)
                        .Select(x => x.RoleId);
                    return Task.FromResult(res);
                }
            );
        }

        [HttpPost, HttpPut, HttpDelete, Route("{roleId}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLock(string roleId)
        {
            var spl = roleId.Split(Constants.SplitterRoleParts, 3);
            if (RequestAppHeaders.Corp != spl[0] || RequestAppHeaders.App != spl[1])
                return CrossAppToken();

            var @lock = !Request.Method.EqualsIgnoreCase(HttpMethods.Delete);

            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatus(new Shared.Domains.Role
                    {
                        RoleId = roleId,
                        Locked = @lock
                    });
                }
            );
        }
    }
}