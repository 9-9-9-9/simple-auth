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
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;
using LocalUserInfo = SimpleAuth.Shared.Domains.LocalUserInfo;
using User = SimpleAuth.Services.Entities.User;

namespace SimpleAuth.Server.Controllers
{
    /// <summary>
    /// Serves requests relate to user management, user authentication/authorization
    /// </summary>
    [Route("api/users")]
    [RequireAppToken]
    public class UserController : BaseController<IUserService, IUserRepository, User>
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IUserValidationService _userValidationService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// DI constructor
        /// </summary>
        public UserController(IServiceProvider serviceProvider, IEncryptionService encryptionService,
            IUserValidationService userValidationService) : base(
            serviceProvider)
        {
            _encryptionService = encryptionService;
            _userValidationService = userValidationService;
            _logger = serviceProvider.ResolveLogger<UserController>();
        }

        /// <summary>
        /// Create user in the specific Corp, the target Corp will be retrieved from 'x-app-token'.
        /// Based on specification of SimpleAuth, user is at Corp's level,
        /// therefore need to register user at Corp in order to use features
        /// </summary>
        /// <param name="model">User information</param>
        /// <response code="201">User created successfully</response>
        /// <response code="400">Request model is invalid</response>
        /// <response code="409">Corp already has that user id</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = Service.GetUser(model.UserId, RequestAppHeaders.Corp);
            if (user != default)
                return Conflict();

            var vr = new Func<ValidationResult>[]
            {
                () => _userValidationService.IsValidUserId(model.UserId),
                () => _userValidationService.IsValidPassword(model.Password),
                () => _userValidationService.IsValidEmail(model.Email),
            }.IsValid();

            if (!vr.IsValid)
                return BadRequest(vr.Message);

            return await ProcedureDefaultResponseIfError(async () =>
            {
                await Service.CreateUserAsync(new Shared.Domains.User
                    {
                        Id = model.UserId
                    },
                    new LocalUserInfo
                    {
                        Corp = RequestAppHeaders.Corp,
                        PlainPassword = model.Password
                    });

                _logger.LogInformation($"User {model.UserId} had been created at {RequestAppHeaders.Corp}");
                return StatusCodes.Status201Created.WithEmpty();
            });
        }

        /// <summary>
        /// Get user's information.
        /// </summary>
        /// <param name="userId">Selected user id</param>
        /// <returns><see cref="SimpleAuth.Shared.Models.ResponseUserModel"/></returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(string userId)
        {
            return await ProcedureDefaultResponseIfError(() =>
                GetBaseResponseUserModelAsync(userId, Service)
                    .ContinueWith(x =>
                        StatusCodes.Status200OK.WithJson(x.Result)
                    )
            );
        }

        /// <summary>
        /// Assign user to specific Permission Group of the same Corp, same App.
        /// In order to grant permission for user, manager has to set permissions for permission group and then assign user to that group if user is not belong to it.
        /// </summary>
        /// <param name="userId">The target user which should be granted permission</param>
        /// <param name="modifyUserPermissionGroupsModel">The target permission groups which should take the user</param>
        /// <response code="200">Assign user to the specific permission groups completed</response>
        /// <response code="400">Request model is invalid</response>
        /// <response code="404">Target user id or permission group not found</response>
        [HttpPost, Route("{userId}/permission-groups")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignUserToGroupsAsync(string userId,
            [FromBody] ModifyUserPermissionGroupsModel modifyUserPermissionGroupsModel)
        {
            if ((modifyUserPermissionGroupsModel?.PermissionGroups?.Length ?? 0) == 0)
                return BadRequest();

            return await ProcedureDefaultResponse(async () =>
                {
                    var user = Service.GetUser(userId, RequestAppHeaders.Corp);
                    if (user == default)
                        throw new EntityNotExistsException(
                            $"{userId} at {RequestAppHeaders.App} of {RequestAppHeaders.Corp}");
                    await Service.AssignUserToGroupsAsync(new Shared.Domains.User
                    {
                        Id = userId
                    }, modifyUserPermissionGroupsModel.PermissionGroups.Select(x => new PermissionGroup
                    {
                        Name = x,
                        Corp = RequestAppHeaders.Corp,
                        App = RequestAppHeaders.App
                    }).ToArray());
                    _logger.LogInformation(
                        $"Assigned user {userId} to groups {string.Join(',', modifyUserPermissionGroupsModel.PermissionGroups)} ({RequestAppHeaders.Corp}.{RequestAppHeaders.App})"
                    );
                }
            );
        }

        /// <summary>
        /// Get the current active roles of an user, but for the current corp and app only, another corp/app are excluded
        /// </summary>
        /// <param name="userId">The target user which should be checked</param>
        /// <returns>Array of <see cref="SimpleAuth.Shared.Models.PermissionModel"/></returns>
        /// <response code="200">Retrieve information without any problem</response>
        /// <response code="404">User id could not be found</response>
        [HttpGet("{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActiveRoles(string userId)
        {
            return await ProcedureDefaultResponseIfError(() =>
                GetBaseResponseUserModelAsync(userId, Service)
                    .ContinueWith(x =>
                        StatusCodes.Status200OK.WithJson(
                            x.Result
                                .ActiveRoles
                                .OrEmpty()
                                .ToArray()
                        )
                    )
            );
        }

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        /// <param name="userId">User to be checked</param>
        /// <param name="roleId">Role to be checked, with full parts, example: corp.app.env.tenant.module.subModules</param>
        /// <param name="verb">Serialized permission (byte value)</param>
        /// <returns>No content but status code is 200 if user has permission, 406 if user doesn't have that permission</returns>
        /// <response code="200">User HAS the required permission</response>
        /// <response code="406">User DOES NOT HAVE the required permission</response>
        /// <response code="404">User is not exists</response>
        [HttpGet, Route("{userId}/roles/{roleId}/{verb}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckUserPermission(string userId, string roleId, string verb)
        {
            return await ProcedureDefaultResponseIfError(() =>
                {
                    var eVerb = verb.Deserialize();
                    _logger.LogInformation($"Checking permission [{roleId}, {eVerb}] for user {userId}");

                    return Service.GetMissingPermissionsAsync(
                        userId,
                        new[] {(roleId, eVerb)},
                        RequestAppHeaders.Corp, RequestAppHeaders.App
                    ).ContinueWith(
                        x => (x.Result.IsAny()
                                ? StatusCodes.Status406NotAcceptable
                                : StatusCodes.Status200OK
                            ).WithEmpty()
                    );
                }
            );
        }

        /// <summary>
        /// Check if user has all permissions which are declared in payload
        /// </summary>
        /// <param name="userId">User to be checked</param>
        /// <param name="permissionModels">Role to be checked, with full parts, example: corp.app.env.tenant.module.subModules, permission is serialized Permission (byte value)</param>
        /// <returns>Status code is 200 without content if user has all required permissions or status code 406 with json array of missing permissions</returns>
        /// <response code="200">User HAS all the required permissions</response>
        /// <response code="406">Any of the permissions user does not have</response>
        /// <response code="404">User is not exists</response>
        [HttpPost, Route("{userId}/roles/_missing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMissingPermissions(string userId, [FromBody] PermissionModels permissionModels)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return await ProcedureDefaultResponseIfError(() =>
                {
                    var roles = permissionModels.Permissions.Select(x => (x.Role, x.Verb.Deserialize())).ToArray();
                    _logger.LogInformation(
                        $"Checking permissions [{string.Join(",", roles.Select(x => $"{x.Role},{x.Item2}"))}] for user {userId}");

                    return Service.GetMissingPermissionsAsync(
                        userId,
                        roles,
                        RequestAppHeaders.Corp, RequestAppHeaders.App
                    ).ContinueWith(
                        x => x.Result.IsAny() 
                            ? StatusCodes.Status406NotAcceptable.WithJson(x.Result.Select(r => r.Cast()).ToArray()) 
                            : StatusCodes.Status200OK.WithEmpty()
                    );
                }
            );
        }

        /// <summary>
        /// Get user's information, if the specified user and password are correct
        /// </summary>
        /// <param name="userId">User id to check</param>
        /// <param name="password">Plain-text password of the user</param>
        /// <returns>If password of user is correct, then user's information will be responded to client</returns>
        /// <response code="200">Password is match and response information</response>
        /// <response code="404">User not found</response>
        /// <response code="412">User does not has password, thus can not be checked</response>
        /// <response code="401">Password mis-match</response>
        [HttpPost("{userId}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckPass(string userId, [FromBody] string password)
        {
            return await ProcedureDefaultResponseIfError(async () =>
            {
                var usr = Repository.Find(userId);
                var localUserInfo = usr?.UserInfos?.FirstOrDefault(x => x.Corp == RequestAppHeaders.Corp);
                if (localUserInfo == null)
                {
                    _logger.LogInformation($"User id {userId} is not exists in {RequestAppHeaders.Corp}");
                    throw new EntityNotExistsException(userId);
                }

                if (localUserInfo.EncryptedPassword.IsBlank())
                    return StatusCodes.Status412PreconditionFailed.WithMessage("User has no password defined");

                var pwdMatch = _encryptionService.Decrypt(localUserInfo.EncryptedPassword).Equals(password);

                if (!pwdMatch)
                {
                    _logger.LogInformation($"Password mis-match for user {userId}");
                    return Unauthorized();
                }

                return await GetUser(userId);
            });
        }

        /// <summary>
        /// Change password of user within Corp
        /// </summary>
        /// <param name="userId">User to change pass</param>
        /// <param name="newPassword">Plain text of the new password</param>
        /// <response code="200">Password updated successfully</response>
        /// <response code="400">Password is malformed</response>
        [HttpPut("{userId}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePass(string userId, [FromBody] string newPassword)
        {
            var vr = _userValidationService.IsValidPassword(newPassword);
            if (!vr.IsValid)
                return StatusCodes.Status400BadRequest.WithMessage(vr.Message);

            return await ProcedureDefaultResponse(async () =>
            {
                await Service.UpdatePasswordAsync(new Shared.Domains.User
                {
                    Id = userId,
                    LocalUserInfos = new[]
                    {
                        new LocalUserInfo
                        {
                            Corp = RequestAppHeaders.Corp,
                            PlainPassword = newPassword
                        },
                    }
                });
            });
        }

        /// <summary>
        /// Lock a specific user
        /// </summary>
        /// <param name="userId">User to be locked</param>
        /// <response code="200">Operation completed successfully</response>
        [HttpPost, Route("{userId}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LockUser(string userId)
        {
            _logger.LogInformation($"LOCK User {userId}");

            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatusAsync(new Shared.Domains.User
                    {
                        Id = userId,
                        LocalUserInfos = new[]
                        {
                            new LocalUserInfo
                            {
                                Corp = RequestAppHeaders.Corp,
                                Locked = true
                            }
                        }
                    });
                }
            );
        }

        /// <summary>
        /// Un-Lock a specific user
        /// </summary>
        /// <param name="userId">User to be unlocked</param>
        /// <response code="200">Operation completed successfully</response>
        [HttpDelete, Route("{userId}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnLockUser(string userId)
        {
            _logger.LogInformation($"UnLock User {userId}");

            return await ProcedureDefaultResponse(async () =>
                {
                    await Service.UpdateLockStatusAsync(new Shared.Domains.User
                    {
                        Id = userId,
                        LocalUserInfos = new[]
                        {
                            new LocalUserInfo
                            {
                                Corp = RequestAppHeaders.Corp,
                                Locked = false
                            }
                        }
                    });
                }
            );
        }
    }
}