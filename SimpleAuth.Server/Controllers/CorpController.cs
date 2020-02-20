using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleAuth.Server.Extensions;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Validation;

namespace SimpleAuth.Server.Controllers
{
    /// <summary>
    /// Controller for managing Corp. By providing a corp-level token as `x-corp-token` header, requester can access features
    /// </summary>
    [Route("api/corp")]
    [RequireCorpToken]
    public class CorpController : BaseController
    {
        private readonly IEncryptionService _encryption;
        private readonly ITokenInfoService _tokenInfoService;
        private readonly IRolePartsValidationService _rolePartsValidationService;
        private readonly ILogger<CorpController> _logger;

        /// <summary>
        /// DI constructor
        /// </summary>
        public CorpController(IServiceProvider serviceProvider, IEncryptionService encryption,
            ITokenInfoService tokenInfoService, IRolePartsValidationService rolePartsValidationService)
            : base(serviceProvider)
        {
            _encryption = encryption;
            _tokenInfoService = tokenInfoService;
            _rolePartsValidationService = rolePartsValidationService;
            _logger = serviceProvider.ResolveLogger<CorpController>();
        }

        /// <summary>
        /// Generate a token, which has to be provided in 'x-app-token' header for some restricted actions
        /// </summary>
        /// <param name="app">Target app to generate token, if app does not exists, a newly one with version 1 will be generated</param>
        /// <param name="public">Query param indicate this is generated for public use, being used by client, those client are restricted to read-only</param>
        /// <returns>A newly created token, with version increased</returns>
        /// <response code="200">Token generated successfully</response>
        /// <response code="400">Specified app is malformed</response>
        /// <response code="428">Token has to be generated without ReadOnly flag set to false (means public != true)</response>
        [HttpGet("_token/{app}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
        public async Task<IActionResult> GenerateAppPermissionToken(string app, [FromQuery] bool @public)
        {
            _logger.LogWarning($"{nameof(GenerateAppPermissionToken)} for application {RequireCorpToken.Corp}.{app}");

            if (!_rolePartsValidationService.IsValidApp(app).IsValid)
                return StatusCodes.Status400BadRequest.WithMessage(nameof(app));

            var corp = RequireCorpToken.Corp;

            if (@public)
            {
                var currentVersion = await _tokenInfoService.GetCurrentVersionAsync(new TokenInfo
                {
                    Corp = corp,
                    App = app
                }, true);

                if (currentVersion < 1)
                    return StatusCodes.Status428PreconditionRequired.WithEmpty();

                var actionResult =
                    StatusCodes.Status200OK.WithMessage(GenerateAppTokenContent(currentVersion, true));

                _logger.LogWarning($"Generated read-only token for {RequireCorpToken.Corp}.{app} at current version {currentVersion}");

                return actionResult;
            }
            else
            {
                var nextTokenVersion = await _tokenInfoService.IncreaseVersionAsync(new TokenInfo
                {
                    Corp = corp,
                    App = app
                });

                var actionResult =
                    StatusCodes.Status200OK.WithMessage(GenerateAppTokenContent(nextTokenVersion, false));

                _logger.LogWarning($"Generated token for {RequireCorpToken.Corp}.{app} version {nextTokenVersion}");

                return actionResult;
            }

            string GenerateAppTokenContent(int version, bool isReadOnly)
            {
                return _encryption.Encrypt(new RequestAppHeaders
                {
                    Header = Constants.Headers.AppPermission,
                    Corp = corp,
                    App = app,
                    Version = version,
                    ReadOnly = isReadOnly
                }.ToJson());
            }
        }
    }
}