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
    [Route("admin")]
    [RequireMasterToken]
    public class AdministrationController : BaseController
    {
        private readonly IEncryptionService _encryption;
        private readonly ITokenInfoService _tokenInfoService;
        private readonly IRolePartsValidationService _rolePartsValidationService;
        private readonly ILogger<AdministrationController> _logger;

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

        [HttpGet("token/{corp}")]
        public async Task<IActionResult> GenerateCorpPermissionToken(string corp)
        {
            _logger.LogWarning($"{nameof(GenerateCorpPermissionToken)} for corp {RequireCorpToken.Corp}");
            
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

            _logger.LogWarning($"Generated token for {RequireCorpToken.Corp} version {nextTokenVersion}");
            
            return actionResult;
        }

        [HttpGet("token/{corp}/{app}")]
        public async Task<IActionResult> GenerateAppPermissionToken(string corp, string app)
        {
            _logger.LogWarning($"{nameof(GenerateAppPermissionToken)} for application {corp}.{app}");
            
            if (!_rolePartsValidationService.IsValidCorp(corp).IsValid)
                return StatusCodes.Status400BadRequest.WithMessage(nameof(corp));
            if (!_rolePartsValidationService.IsValidApp(app).IsValid)
                return StatusCodes.Status400BadRequest.WithMessage(nameof(app));
            
            var nextTokenVersion = await _tokenInfoService.IncreaseVersionAsync(new TokenInfo
            {
                Corp = corp,
                App = app
            });

            var actionResult = StatusCodes.Status201Created.WithMessage(_encryption.Encrypt(new RequestAppHeaders
            {
                Header = Constants.Headers.AppPermission,
                Corp = corp,
                App = app,
                Version = nextTokenVersion
            }.ToJson()));
            
            _logger.LogWarning($"Generated token for {RequireCorpToken.Corp}.{app} version {nextTokenVersion}");

            return actionResult;
        }

        [HttpGet("encrypt")]
        public IActionResult EncryptPlainText([FromQuery, Required] string data)
        {
            _logger.LogWarning("Received an encryption request");
            
            if (data.IsBlank())
                return StatusCodes.Status400BadRequest.WithMessage(nameof(data));
            return StatusCodes.Status200OK.WithMessage(_encryption.Encrypt(data));
        }

        [HttpGet("decrypt")]
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