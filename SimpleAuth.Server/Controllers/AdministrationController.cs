using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleAuth.Core.Extensions;
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
    /// Master controller for administration. By providing a master token as `x-master-token` header, requester can access ultimate features
    /// </summary>
    [Route("admin")]
    [RequireMasterToken]
    public class AdministrationController : BaseController
    {
        private readonly IEncryptionService _encryption;
        private readonly ITokenInfoService _tokenInfoService;
        private readonly IRolePartsValidationService _rolePartsValidationService;
        private readonly ILogger<AdministrationController> _logger;

        /// <summary>
        /// DI constructor
        /// </summary>
        public AdministrationController(IServiceProvider serviceProvider,
            IEncryptionService encryption,
            ITokenInfoService tokenInfoService,
            IRolePartsValidationService rolePartsValidationService) : base(serviceProvider)
        {
            _encryption = encryption;
            _tokenInfoService = tokenInfoService;
            _rolePartsValidationService = rolePartsValidationService;
            _logger = serviceProvider.ResolveLogger<AdministrationController>();
        }

        
        /// <summary>
        /// Generate a token, which has to be provided in 'x-app-token' header for some restricted actions
        /// </summary>
        /// <param name="corp">Target corp to generate token</param>
        /// <param name="app">Target app to generate token, if app does not exists, a newly one with version 1 will be generated</param>
        /// <param name="public">Query param indicate this is generated for public use, being used by client, those client are restricted to read-only</param>
        /// <returns>A newly created token, with version increased</returns>
        /// <response code="200">Token generated successfully</response>
        /// <response code="400">Specified corp/app is malformed</response>
        /// <response code="428">Token has to be generated without ReadOnly flag set to false (means public != true)</response>
        [HttpGet("token/{corp}/{app}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
        public async Task<IActionResult> GenerateAppPermissionToken(string corp, string app, [FromQuery] bool @public)
        {
            _logger.LogWarning($"{nameof(GenerateAppPermissionToken)} for application {corp}.{app}");
            
            if (!_rolePartsValidationService.IsValidCorp(corp).IsValid)
                return StatusCodes.Status400BadRequest.WithMessage(nameof(corp));
            if (!_rolePartsValidationService.IsValidApp(app).IsValid)
                return StatusCodes.Status400BadRequest.WithMessage(nameof(app));
            
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

                _logger.LogWarning($"Generated read-only token for {corp}.{app} at current version {currentVersion}");

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

        /// <summary>
        /// Generate a token, which has to be provided in 'x-corp-token' header for some restricted actions
        /// </summary>
        /// <param name="corp">Target corp to generate token, if app does not exists, a newly one with version 1 will be generated</param>
        /// <returns>A newly created token, with version increased</returns>
        /// <response code="200">Token generated successfully</response>
        /// <response code="400">Specified corp is malformed</response>
        [HttpGet("token/{corp}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateCorpPermissionToken(string corp)
        {
            _logger.LogWarning($"{nameof(GenerateCorpPermissionToken)} for corp {corp}");
            
            if (!_rolePartsValidationService.IsValidCorp(corp).IsValid)
                return StatusCodes.Status400BadRequest.WithMessage(nameof(corp));
            
            var nextTokenVersion = await _tokenInfoService.IncreaseVersionAsync(new TokenInfo
            {
                Corp = corp,
                App = string.Empty
            });

            var actionResult = StatusCodes.Status201Created.WithMessage(_encryption.Encrypt(new RequireCorpToken
            {
                Header = Constants.Headers.CorpPermission,
                Corp = corp,
                Version = nextTokenVersion
            }.ToJson()));

            _logger.LogWarning($"Generated token for {corp} version {nextTokenVersion}");
            
            return actionResult;
        }

        /// <summary>
        /// Encrypt text using the master public key, use at your own risk
        /// </summary>
        /// <param name="data">Data to be encrypted</param>
        /// <returns>Data which were encrypted by public key, default encryption method using RSA 2048</returns>
        /// <response code="200">Encrypted text from input data</response>
        /// <response code="400">Input data was blank</response>
        [HttpGet("encrypt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult EncryptPlainText([FromQuery, Required] string data)
        {
            _logger.LogWarning("Received an encryption request");
            
            if (data.IsBlank())
                return StatusCodes.Status400BadRequest.WithMessage(nameof(data));
            return StatusCodes.Status200OK.WithMessage(_encryption.Encrypt(data));
        }

        
        /// <summary>
        /// Decrypt data using the master private key, use at your own risk
        /// </summary>
        /// <param name="data">Data to be decrypted</param>
        /// <returns>Data which were decrypted using master private key, default encryption method using RSA 2048</returns>
        /// <response code="200">Decrypted value from input data</response>
        /// <response code="400">Input data was blank</response>
        /// <response code="417">Error occured during decryption. Perhaps input data was incorrect, something which was not encrypted using master public/private key pair</response>
        [HttpGet("decrypt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status417ExpectationFailed)]
        public IActionResult DecryptData([FromQuery, Required] string data)
        {
            _logger.LogWarning("Received an decryption request");
            
            if (data.IsBlank())
                return StatusCodes.Status400BadRequest.WithMessage(nameof(data));
            try
            {
                return StatusCodes.Status200OK.WithMessage(_encryption.Decrypt(data));
            }
            catch (Exception e)
            {
                e.Write();
                return StatusCodes.Status417ExpectationFailed.WithMessage(e.Message);
            }
        }
    }
}