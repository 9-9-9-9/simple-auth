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
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;
using LocalUserInfo = SimpleAuth.Shared.Domains.LocalUserInfo;
using RoleGroup = SimpleAuth.Shared.Domains.RoleGroup;

namespace SimpleAuth.Server.Controllers
{
    [Route("api/users")]
    [RequireAppToken]
    public class UserController : BaseController<IUserService, IUserRepository, User>
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IUserValidationService _userValidationService;
        private readonly ILogger<UserController> _logger;

        public UserController(IServiceProvider serviceProvider, IEncryptionService encryptionService,
            IUserValidationService userValidationService) : base(
            serviceProvider)
        {
            _encryptionService = encryptionService;
            _userValidationService = userValidationService;
            _logger = serviceProvider.ResolveLogger<UserController>();
        }

        [HttpPost, Route("{userId}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LockUser(string userId)
        {
            var @lock = !Request.Method.EqualsIgnoreCase(HttpMethods.Delete);

            _logger.LogInformation($"Update LOCK status of UserId {userId} to {@lock}");

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
                                Locked = @lock
                            }
                        }
                    });
                }
            );
        }

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
                    return StatusCodes.Status412PreconditionFailed.WithMessage($"User has no password defined");

                var pwdMatch = _encryptionService.Decrypt(localUserInfo.EncryptedPassword).Equals(password);

                if (!pwdMatch)
                {
                    _logger.LogInformation($"Password miss-match for user {userId}");
                    return Unauthorized();
                }

                return await GetUser(userId);
            });
        }

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

        [HttpGet, Route("{userId}/roles/{roleId}/{permission}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckUserPermission(string userId, string roleId, string permission)
        {
            return await ProcedureDefaultResponseIfError(() =>
                {
                    var ePermission = permission.Deserialize();
                    _logger.LogInformation($"Checking permission [{roleId}, {ePermission}] for user {userId}");

                    return Service.IsHaveActivePermissionAsync(
                        userId,
                        roleId,
                        ePermission,
                        RequestAppHeaders.Corp, RequestAppHeaders.App
                    ).ContinueWith(
                        x => (x.Result
                                ? StatusCodes.Status200OK
                                : StatusCodes.Status406NotAcceptable
                            ).WithEmpty()
                    );
                }
            );
        }

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

        [HttpPost, Route("{userId}/role-groups")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignUserToGroupsAsync(string userId,
            [FromBody] ModifyUserRoleGroupsModel modifyUserRoleGroupsModel)
        {
            if ((modifyUserRoleGroupsModel?.RoleGroups?.Length ?? 0) == 0)
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
                    }, modifyUserRoleGroupsModel.RoleGroups.Select(x => new RoleGroup
                    {
                        Name = x,
                        Corp = RequestAppHeaders.Corp,
                        App = RequestAppHeaders.App
                    }).ToArray());
                    _logger.LogInformation(
                        $"Assigned user {userId} to groups {string.Join(',', modifyUserRoleGroupsModel.RoleGroups)} ({RequestAppHeaders.Corp}.{RequestAppHeaders.App})"
                    );
                }
            );
        }
    }
}