using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    /// <summary>
    /// Reserved Endpoint for serving requests relate to Google, such as sign-in using OAuth token
    /// </summary>
    [Microsoft.AspNetCore.Components.Route("api/external/google")]
    [RequireAppToken]
    public class GoogleController : BaseController<IUserService, IUserRepository, User>
    {
        private readonly IGoogleService _googleService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// DI constructor
        /// </summary>
        public GoogleController(IServiceProvider serviceProvider,
            IGoogleService googleService) : base(
            serviceProvider)
        {
            _googleService = googleService;
            _logger = serviceProvider.ResolveLogger<UserController>();
        }

        /// <summary>
        /// User can get roles and permissions via Google OAuth, just by sending token and request some additional checking
        /// </summary>
        /// <returns>User information, include active roles</returns>
        /// <response code="200">Checked everything with a good result</response>
        /// <response code="404">User had not been registered in the specified Corp</response>
        /// <response code="423">User had been locked in the specified Corp</response>
        /// <response code="412">Cannot Google for verifying token</response>
        /// <response code="406">There are some mis-match/invalidated information from token info that does not fit requirement</response>
        [HttpPost("token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
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
                    _logger.LogWarning(ex, "Google token verification issue");
                    return StatusCodes.Status412PreconditionFailed.WithMessage(ex.Message);
                }

                await _googleService.VerifyRequestAsync(RequestAppHeaders.Corp, form, ggToken);

                var expiryDate = new DateTime(1970, 1, 1, 1, 1, 1, DateTimeKind.Utc);
                expiryDate = expiryDate.AddSeconds(int.Parse(ggToken.Exp));
                
                var model = await GetBaseResponseUserModelAsync(form.Email, Service);
                model.GoogleToken = ggToken;
                model.ExpireAt(expiryDate);

                _logger.LogInformation($"Access granted for {form.Email} via Google Token");

                return StatusCodes.Status200OK.WithJson(model);
            });
        }
    }
}