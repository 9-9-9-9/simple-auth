using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Extensions;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;
using SimpleAuth.Shared.Validation;
using PermissionGroup = SimpleAuth.Services.Entities.PermissionGroup;

namespace SimpleAuth.Server.Controllers
{
    /// <summary>
    /// Create and manage Role Groups, which contains a lot of roles with permission, then user can be assigned into Role Group to inherit there permissions
    /// </summary>
    [Route("api/role-groups")]
    [RequireAppToken]
    public class RoleGroupsController : BaseController<IPermissionGroupService, IPermissionGroupRepository, PermissionGroup>
    {
        private readonly IPermissionGroupValidationService _permissionGroupValidation;
        private readonly ILogger<RoleGroupsController> _logger;

        /// <summary>
        /// DI constructor
        /// </summary>
        public RoleGroupsController(IServiceProvider serviceProvider,
            IPermissionGroupValidationService permissionGroupValidation) :
            base(serviceProvider)
        {
            _permissionGroupValidation = permissionGroupValidation;
            _logger = serviceProvider.ResolveLogger<RoleGroupsController>();
        }

        /// <summary>
        /// Create a role group, which belong to Corp and App specified in x-app-token header
        /// </summary>
        /// <param name="model">Details of the new group</param>
        /// <response code="201">Role Group had been created successfully</response>
        /// <response code="400">Request model is malformed</response>
        /// <response code="409">The group name is already exists within app</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddRoleGroup([FromBody] CreatePermissionGroupModel model)
        {
            if (model.Name.IsBlank())
                model.Name = $"{model.Corp}-{model.App}-{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8)}"
                    .NormalizeInput();

            if (!ModelState.IsValid)
                return BadRequest(nameof(ModelState));

            if (model.Corp != RequestAppHeaders.Corp || model.App != RequestAppHeaders.App)
                return CrossAppToken();

            var vr = _permissionGroupValidation.IsValid(model);
            if (!vr.IsValid)
                return BadRequest(vr.Message);

            return await ProcedureResponseForPersistAction(async () =>
                await Service.AddGroupAsync(model)
            );
        }

        /// <summary>
        /// Get specific role group information
        /// </summary>
        /// <param name="name">Name of the role group</param>
        /// <returns>Information of the role group, refer domain model <see cref="Shared.Domains.PermissionGroup"/></returns>
        /// <response code="200">Role Group information retrieved successfully</response>
        /// <response code="404">Role Group could not be found</response>
        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleGroup(string name)
        {
            return await ProcedureResponseForLookUp(() =>
                FindRoleGroupAsync(name, RequestAppHeaders.Corp, RequestAppHeaders.App)
            );
        }

        /// <summary>
        /// Get all the role with permission of the role group. Locked roles also be included
        /// </summary>
        /// <param name="groupName">Name of the group to be retrieved</param>
        /// <returns>All roles, locked roles also included</returns>
        /// <response code="200">Role Group information retrieved successfully</response>
        /// <response code="404">Role Group could not be found</response>
        [HttpGet("{groupName}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoles(string groupName)
        {
            return await ProcedureResponseForArrayLookUp(() =>
                FindRoleGroupAsync(groupName, RequestAppHeaders.Corp, RequestAppHeaders.App)
                    .ContinueWith(x =>
                        x.Result.Permissions.OrEmpty().Select(PermissionModel.Cast)
                    )
            );
        }

        /// <summary>
        /// Add role to the role group
        /// </summary>
        /// <param name="groupName">Target role group to be expanded</param>
        /// <param name="model">Permissions to be added to role group</param>
        /// <response code="200">Added without any problem</response>
        /// <response code="404">Role Group/Role Id could not be found</response>
        /// <response code="400">Request model is malformed</response>
        [HttpPost, Route("{groupName}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddRoles(
            string groupName,
            [FromBody] PermissionModels model)
        {
            if (!ModelState.IsValid)
                return BadRequest(nameof(ModelState));

            return await ProcedureDefaultResponse(async () =>
            {
                var group = await FindRoleGroupAsync(groupName, RequestAppHeaders.Corp, RequestAppHeaders.App);
                await Service.AddPermissionsToGroupAsync(group, model.Permissions);
            });
        }

        /// <summary>
        /// Remove roles from role group
        /// </summary>
        /// <param name="groupName">Target role group</param>
        /// <param name="roles">[Query] Role Ids, which will be removed from role group</param>
        /// <param name="all">[Query] Indicate that all roles of the role groups should be completely removed</param>
        /// <response code="200">Removed without any problem</response>
        /// <response code="404">Role Group/Role Id could not be found</response>
        /// <response code="400">Either of query parameters are malformed</response>
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
                var group = new Shared.Domains.PermissionGroup
                {
                    Name = groupName,
                    Corp = RequestAppHeaders.Corp,
                    App = RequestAppHeaders.App
                };
                if (all)
                    await Service.DeleteAllPermissionsFromGroupAsync(group);
                else
                    await Service.DeletePermissionsFromGroupAsync(group, roles.Select(RoleUtils.UnMerge)
                        .Select(tp => new PermissionModel
                        {
                            Role = tp.Item1,
                            Verb = tp.Item2.Serialize()
                        })
                        .ToArray());
            });
        }

        /// <summary>
        /// Search role groups which has name matching with search term
        /// </summary>
        /// <param name="term">[Query] Search term, to be compared with role group name</param>
        /// <param name="skip">[Query] Used for paging</param>
        /// <param name="take">[Query] Used for paging</param>
        /// <returns>Array of name of groups which have name match with search term</returns>
        /// <response code="200">Result found</response>
        /// <response code="400">Either of requesting query param term/skip/take is malformed</response>
        /// <response code="404">No result found</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FindRoleGroups(
            [FromQuery] string term,
            [FromQuery] int? skip,
            [FromQuery] int? take)
        {
            return await ProcedureResponseForLookUpArrayUsingTerm(term, skip, take,
                findOptions =>
                    Task.FromResult(
                        Service
                            .SearchGroups(term, RequestAppHeaders.Corp, RequestAppHeaders.App, findOptions)
                            .Select(x => x.Name)
                    )
            );
        }

        /// <summary>
        /// Lock specific role group
        /// </summary>
        /// <param name="groupName">Name of the role group which should be locked</param>
        /// <response code="200">Operation had been completed successfully</response>
        /// <response code="404">Role group could not be found</response>
        [HttpPost("{groupName}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LockRoleGroup(string groupName)
        {
            _logger.LogInformation($"Lock role group {groupName}");
            
            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatusAsync(new Shared.Domains.PermissionGroup
                    {
                        Name = groupName,
                        Corp = RequestAppHeaders.Corp,
                        App = RequestAppHeaders.App,
                        Locked = true
                    });
                }
            );
        }

        /// <summary>
        /// UnLock specific role group
        /// </summary>
        /// <param name="groupName">Name of the role group which should be unlocked</param>
        /// <response code="200">Operation had been completed successfully</response>
        /// <response code="404">Role group could not be found</response>
        [HttpDelete("{groupName}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlockRoleGroup(string groupName)
        {
            _logger.LogInformation($"Unlock role group {groupName}");
            
            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatusAsync(new Shared.Domains.PermissionGroup
                    {
                        Name = groupName,
                        Corp = RequestAppHeaders.Corp,
                        App = RequestAppHeaders.App,
                        Locked = false
                    });
                }
            );
        }

        private async Task<Shared.Domains.PermissionGroup> FindRoleGroupAsync(string name, string corp, string app)
        {
            var group = await Service.GetGroupByNameAsync(name, corp, app);
            if (group == null)
                throw new EntityNotExistsException(name);
            return group;
        }
    }
}