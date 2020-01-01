using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Extensions;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Server.Services;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Validation;
using LocalUserInfo = SimpleAuth.Shared.Domains.LocalUserInfo;

namespace SimpleAuth.Server.Controllers
{
    [Route("api/users")]
    [RequireAppToken]
    public class UserController : BaseController<IUserService, IUserRepository, User>
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IGoogleService _googleService;
        private readonly IUserValidationService _userValidationService;

        public UserController(IServiceProvider serviceProvider, IEncryptionService encryptionService,
            IGoogleService googleService, IUserValidationService userValidationService) : base(
            serviceProvider)
        {
            _encryptionService = encryptionService;
            _googleService = googleService;
            _userValidationService = userValidationService;
        }

        [HttpPost, HttpPut, HttpPatch, Route("{userId}/lock")]
        public async Task<IActionResult> LockUser(string userId)
        {
            var @lock = !Request.Method.EqualsIgnoreCase(HttpMethods.Delete);

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
        public IActionResult CheckPass(string userId, [FromQuery] string password)
        {
            return ProcedureDefaultResponseIfError(() =>
            {
                var usr = Repository.Find(userId);
                var localUserInfo = usr?.UserInfos?.FirstOrDefault(x => x.Corp == RequestAppHeaders.Corp);
                if (localUserInfo == null)
                    throw new EntityNotExistsException(userId);

                if (localUserInfo.EncryptedPassword.IsBlank())
                    return StatusCodes.Status412PreconditionFailed.WithEmpty();

                var pwdMatch = _encryptionService.Decrypt(localUserInfo.EncryptedPassword).Equals(password);

                if (!pwdMatch)
                    return Unauthorized();

                return GetUser(userId);
            });
        }

        [HttpPut, HttpPatch("{userId}/password")]
        public async Task<IActionResult> ChangePass(string userId, [FromQuery] string newPassword)
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

        [HttpPost("{emailAsUserId}/google")]
        public async Task<IActionResult> CheckGoogleToken(string emailAsUserId, [FromBody] LoginByGoogleRequest form)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (emailAsUserId != form.Email)
                return BadRequest();

            return await ProcedureDefaultResponseIfError(async () =>
            {
                var user = Service.GetUser(emailAsUserId, RequestAppHeaders.Corp);
                var localUserInfo = user?.LocalUserInfos?.FirstOrDefault(x => x.Corp == RequestAppHeaders.Corp);
                if (localUserInfo == null)
                    throw new EntityNotExistsException($"{emailAsUserId} at {RequestAppHeaders.Corp}");
                if (localUserInfo.Locked)
                    throw new AccessLockedEntityException($"{user.Id} at {localUserInfo.Corp}");

                GoogleTokenResponseResult ggToken;

                try
                {
                    ggToken = await _googleService.GetInfoAsync(form.GoogleToken);
                }
                catch (SimpleAuthException e)
                {
                    return StatusCodes.Status412PreconditionFailed.WithMessage(e.Message);
                }

                if (!emailAsUserId.EqualsIgnoreCase(ggToken.Email))
                    throw new DataVerificationMismatchException(
                        $"This token belong to {ggToken.Email} which is different than yours user id {emailAsUserId}"
                    );

                if (!form.VerifyWithClientId.IsBlank())
                    if (!form.VerifyWithClientId.EqualsIgnoreCase(ggToken.Aud))
                        throw new DataVerificationMismatchException(
                            $"This token is rejected, it's not belong to client id {form.VerifyWithClientId}"
                        );

                if (!form.VerifyWithGSuite.IsBlank())
                    if (!form.VerifyWithGSuite.EqualsIgnoreCase(ggToken.Hd))
                        throw new DataVerificationMismatchException(
                            $"This token is rejected, it's not belong to GSuite domain {form.VerifyWithGSuite}"
                        );

                var expiryDate = new DateTime(1970, 1, 1, 1, 1, 1, DateTimeKind.Utc);
                expiryDate = expiryDate.AddSeconds(ggToken.Exp);

                if (DateTime.UtcNow > expiryDate)
                    return StatusCodes.Status406NotAcceptable.WithMessage(
                        "Token already expired"
                    );

                var model = GetBaseResponseUserModel(emailAsUserId);
                model.GoogleToken = ggToken;
                model.ExpireAt(expiryDate);

                return ReturnResponseUserModel(model);
            });
        }

        [HttpGet("{userId}/roles")]
        public IActionResult GetActiveRoles(string userId)
        {
            return GetUser(userId);
        }

        [HttpGet("{userId}")]
        public IActionResult GetUser(string userId)
        {
            return ProcedureDefaultResponseIfError(() => ReturnResponseUserModel(GetBaseResponseUserModel(userId)));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = Service.GetUser(model.UserId, RequestAppHeaders.Corp);
            if (user != null)
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

                return StatusCodes.Status201Created.WithEmpty();
            });
        }

        private ResponseUserModel GetBaseResponseUserModel(string userId)
        {
            var user = Service.GetUser(userId, RequestAppHeaders.Corp);
            if (user == null)
                throw new EntityNotExistsException(userId);

            var activeRoles = Service.GetActiveRoles(userId, RequestAppHeaders.Corp, RequestAppHeaders.App);
            return new ResponseUserModel
            {
                Id = userId,
                Corp = RequestAppHeaders.Corp,
                ActiveRoles = activeRoles.OrEmpty().Select(x => new RoleModel
                {
                    Role = x.RoleId,
                    Permission = x.Permission.Serialize()
                }).ToArray()
            };
        }

        private IActionResult ReturnResponseUserModel(ResponseUserModel model)
        {
            return (
                model.ActiveRoles.IsAny()
                    ? StatusCodes.Status200OK
                    : StatusCodes.Status204NoContent
            ).WithJson(model);
        }
    }
}