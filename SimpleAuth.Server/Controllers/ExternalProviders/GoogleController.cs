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
using SimpleAuth.Server.Services;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Server.Controllers
{
    [Microsoft.AspNetCore.Components.Route("api/external/google")]
    [RequireAppToken]
    public class GoogleController : BaseController<IUserService, IUserRepository, User>
    {
        private readonly IGoogleService _googleService;
        private readonly ILogger<UserController> _logger;

        public GoogleController(IServiceProvider serviceProvider,
            IGoogleService googleService) : base(
            serviceProvider)
        {
            _googleService = googleService;
            _logger = serviceProvider.ResolveLogger<UserController>();
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetUserByGoogleToken([FromBody] LoginByGoogleRequest form)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return await ProcedureDefaultResponseIfError(async () =>
            {
                var user = Service.GetUser(form.Email, RequestAppHeaders.Corp);
                var localUserInfo = user?.LocalUserInfos?.FirstOrDefault(x => x.Corp == RequestAppHeaders.Corp);
                if (localUserInfo == null)
                    throw new EntityNotExistsException($"{form.Email} at {RequestAppHeaders.Corp}");

                if (localUserInfo.Locked)
                    throw new AccessLockedEntityException($"{user.Id} at {localUserInfo.Corp}");

                GoogleTokenResponseResult ggToken;

                try
                {
                    ggToken = await _googleService.GetInfoAsync(form.GoogleToken);
                }
                catch (SimpleAuthException ex)
                {
                    _logger.LogWarning(ex, $"Google token verification issue");
                    return StatusCodes.Status412PreconditionFailed.WithMessage(ex.Message);
                }

                if (!form.Email.EqualsIgnoreCase(ggToken.Email))
                    throw new DataVerificationMismatchException(
                        $"This token belong to {ggToken.Email} which is different than email provided in payload {form.Email}"
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
                {
                    _logger.LogWarning($"User {form.Email} provided an expired google token");
                    return StatusCodes.Status406NotAcceptable.WithMessage(
                        "Token already expired"
                    );
                }

                var model = await GetBaseResponseUserModelAsync(form.Email, Service);
                model.GoogleToken = ggToken;
                model.ExpireAt(expiryDate);

                _logger.LogInformation($"Access granted for {form.Email} via Google Token");

                return StatusCodes.Status200OK.WithJson(model);
            });
        }
    }
}