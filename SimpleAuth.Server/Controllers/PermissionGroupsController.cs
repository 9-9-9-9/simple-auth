using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Extensions;
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
    /// <summary>
    /// Create and manage Permission Groups, which contains a lot of permissions, then user can be assigned into Permission Group to inherit there permissions
    /// </summary>
    [Route("api/permission-groups")]
    [RequireAppToken]
    public class PermissionGroupsController : BaseController<IPermissionGroupService, IPermissionGroupRepository, PermissionGroup>
    {
        private readonly IPermissionGroupValidationService _permissionGroupValidation;
        private readonly ILogger<PermissionGroupsController> _logger;

        /// <summary>
        /// DI constructor
        /// </summary>
        public PermissionGroupsController(IServiceProvider serviceProvider,
            IPermissionGroupValidationService permissionGroupValidation) :
            base(serviceProvider)
        {
            _permissionGroupValidation = permissionGroupValidation;
            _logger = serviceProvider.ResolveLogger<PermissionGroupsController>();
        }

        /// <summary>
        /// Create a permission group, which belong to Corp and App specified in x-app-token header
        /// </summary>
        /// <param name="model">Details of the new group</param>
        /// <response code="201">Permission Group had been created successfully</response>
        /// <response code="400">Request model is malformed</response>
        /// <response code="409">The group name is already exists within app</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddPermissionGroup([FromBody] CreatePermissionGroupModel model)
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
        /// Get specific permission group information
        /// </summary>
        /// <param name="groupName">Name of the permission group</param>
        /// <returns>Information of the permission group, refer domain model <see cref="Shared.Domains.PermissionGroup"/></returns>
        /// <response code="200">Permission Group information retrieved successfully</response>
        /// <response code="404">Permission Group could not be found</response>
        [HttpGet("{groupName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionGroup(string groupName)
        {
            return await ProcedureResponseForLookUp(() =>
                FindPermissionGroupAsync(groupName, RequestAppHeaders.Corp, RequestAppHeaders.App)
            );
        }

        /// <summary>
        /// Update permission group Name and Description
        /// </summary>
        /// <param name="groupName">Name of the permission group to be updated</param>
        /// <param name="updatePermissionGroupInfoModel">Contains information to be updated, null value fields means not update</param>
        /// <response code="200">Permission Group information retrieved successfully</response>
        /// <response code="404">Permission Group could not be found</response>
        [HttpPost("{groupName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePermissionGroupInfo(string groupName, 
            [FromBody] UpdatePermissionGroupInfoModel updatePermissionGroupInfoModel)
        {
            return await ProcedureDefaultResponse(async () =>
                await Service.UpdateGroupInfoAsync(new Shared.Domains.PermissionGroup
                {
                    Name = groupName,
                }, RequestAppHeaders.Corp, RequestAppHeaders.App, new Shared.Domains.PermissionGroup
                {
                    Name = updatePermissionGroupInfoModel.Name.TrimToNull(),
                    Description = updatePermissionGroupInfoModel.Description.TrimToNull()
                })
            );
        }

        /// <summary>
        /// Get all the permissions of the group. Locked permissions also be included
        /// </summary>
        /// <param name="groupName">Name of the group to be retrieved</param>
        /// <returns>All roles, locked roles also included</returns>
        /// <response code="200">Permission Group information retrieved successfully</response>
        /// <response code="404">Permission Group could not be found</response>
        [HttpGet("{groupName}/_permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissions(string groupName)
        {
            return await ProcedureResponseForArrayLookUp(() =>
                FindPermissionGroupAsync(groupName, RequestAppHeaders.Corp, RequestAppHeaders.App)
                    .ContinueWith(x =>
                        x.Result.Permissions.OrEmpty().Select(PermissionModel.Cast)
                    )
            );
        }

        /// <summary>
        /// Add permission to the group
        /// </summary>
        /// <param name="groupName">Target group to be expanded</param>
        /// <param name="model">Permissions to be added to group</param>
        /// <response code="200">Added without any problem</response>
        /// <response code="404">Permission Group/Role Id could not be found</response>
        /// <response code="400">Request model is malformed</response>
        [HttpPost, Route("{groupName}/_permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddPermissions(
            string groupName,
            [FromBody] PermissionModels model)
        {
            if (!ModelState.IsValid)
                return BadRequest(nameof(ModelState));

            return await ProcedureDefaultResponse(async () =>
            {
                var group = await FindPermissionGroupAsync(groupName, RequestAppHeaders.Corp, RequestAppHeaders.App);
                await Service.AddPermissionsToGroupAsync(group, model.Permissions);
            });
        }

        /// <summary>
        /// Remove permissions from group
        /// </summary>
        /// <param name="groupName">Target group</param>
        /// <param name="permissions">[Query] Role Ids, which will be removed from the group</param>
        /// <param name="all">[Query] Indicate that all permissions of the groups should be completely removed</param>
        /// <response code="200">Removed without any problem</response>
        /// <response code="404">Permission Group/Role Id could not be found</response>
        /// <response code="400">Either of query parameters are malformed</response>
        [HttpDelete, Route("{groupName}/_permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePermissions(
            string groupName,
            [FromQuery] string[] permissions,
            [FromQuery] bool all)
        {
            if (all && permissions.IsAny())
                return BadRequest();

            if (!all && !permissions.IsAny())
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
                    await Service.DeletePermissionsFromGroupAsync(group, permissions.Select(RoleUtils.UnMerge)
                        .Select(tp => new PermissionModel
                        {
                            Role = tp.Item1,
                            Verb = tp.Item2.Serialize()
                        })
                        .ToArray());
            });
        }

        /// <summary>
        /// Search permission groups which has name matching with search term
        /// </summary>
        /// <param name="term">[Query] Search term, to be compared with group name</param>
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
        public async Task<IActionResult> FindPermissionGroups(
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
        /// Lock specific permission group
        /// </summary>
        /// <param name="groupName">Name of the permission group which should be locked</param>
        /// <response code="200">Operation had been completed successfully</response>
        /// <response code="404">Permission group could not be found</response>
        [HttpPost("{groupName}/_lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LockPermissionGroup(string groupName)
        {
            _logger.LogInformation($"Lock permission group {groupName}");
            
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
        /// UnLock specific permission group
        /// </summary>
        /// <param name="groupName">Name of the permission group which should be unlocked</param>
        /// <response code="200">Operation had been completed successfully</response>
        /// <response code="404">Permission Group could not be found</response>
        [HttpDelete("{groupName}/_lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlockPermissionGroup(string groupName)
        {
            _logger.LogInformation($"Unlock permission group {groupName}");
            
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

        private async Task<Shared.Domains.PermissionGroup> FindPermissionGroupAsync(string name, string corp, string app)
        {
            var group = await Service.GetGroupByNameAsync(name, corp, app);
            if (group == null)
                throw new EntityNotExistsException(name);
            return group;
        }
    }
}