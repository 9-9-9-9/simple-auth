using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Extensions;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;

namespace SimpleAuth.Server.Controllers
{
    /// <summary>
    /// Create and manage roles, this role will be added into Role Group therefore can be assigned to user
    /// </summary>
    [Route("api/roles")]
    [RequireAppToken]
    public class RolesController : BaseController<IRoleService, IRoleRepository, Role>
    {
        private readonly IRoleValidationService _roleValidationService;
        private readonly ILogger<RolesController> _logger;

        /// <summary>
        /// DI constructor
        /// </summary>
        public RolesController(IServiceProvider serviceProvider, IRoleValidationService roleValidationService) : base(
            serviceProvider)
        {
            _roleValidationService = roleValidationService;
            _logger = serviceProvider.ResolveLogger<RolesController>();
        }

        /// <summary>
        /// Add a new role, which belong to a specific Corp and App (thus these parts can not be wildcard '*')
        /// </summary>
        /// <param name="model">Model of role to be created. Corp and App can not be wildcard thus does not accept * as value, the remaining parts are allowed to be wildcard</param>
        /// <response code="201">Role had been created successfully</response>
        /// <response code="400">Request model is malformed</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
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

        /// <summary>
        /// Find a role using a search term
        /// </summary>
        /// <param name="term">[Query] Term to be used for searching operation</param>
        /// <param name="skip">[Query] Used for paging</param>
        /// <param name="take">[Query] Used for paging</param>
        /// <returns>Array of matching roles</returns>
        /// <response code="200">Result found</response>
        /// <response code="400">Either of requesting query param term/skip/take is malformed</response>
        /// <response code="404">No result found</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Lock specific role
        /// </summary>
        /// <param name="roleId">Id of the role which should be locked</param>
        /// <response code="200">Operation had been completed successfully</response>
        /// <response code="404">Role could not be found</response>
        [HttpPost, Route("{roleId}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LockRole(string roleId)
        {
            var spl = roleId.Split(Constants.SplitterRoleParts, 3);
            if (RequestAppHeaders.Corp != spl[0] || RequestAppHeaders.App != spl[1])
                return CrossAppToken();

            _logger.LogInformation($"Lock role {roleId}");
            
            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatus(new Shared.Domains.Role
                    {
                        RoleId = roleId,
                        Locked = true
                    });
                }
            );
        }

        /// <summary>
        /// Unlock specific role
        /// </summary>
        /// <param name="roleId">Id of the role which should be unlocked</param>
        /// <response code="200">Operation had been completed successfully</response>
        /// <response code="404">Role could not be found</response>
        [HttpDelete, Route("{roleId}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLock(string roleId)
        {
            var spl = roleId.Split(Constants.SplitterRoleParts, 3);
            if (RequestAppHeaders.Corp != spl[0] || RequestAppHeaders.App != spl[1])
                return CrossAppToken();

            _logger.LogInformation($"Unlock role {roleId}");
            
            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatus(new Shared.Domains.Role
                    {
                        RoleId = roleId,
                        Locked = false
                    });
                }
            );
        }
    }
}