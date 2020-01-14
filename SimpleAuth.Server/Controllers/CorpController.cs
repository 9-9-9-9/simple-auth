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
    [Route("corp")]
    [RequireCorpToken]
    public class CorpController : BaseController
    {
        private readonly IEncryptionService _encryption;
        private readonly ITokenInfoService _tokenInfoService;
        private readonly IRolePartsValidationService _rolePartsValidationService;
        private readonly ILogger<CorpController> _logger;

        public CorpController(IServiceProvider serviceProvider, IEncryptionService encryption,
            ITokenInfoService tokenInfoService, IRolePartsValidationService rolePartsValidationService) 
            : base(serviceProvider)
        {
            _encryption = encryption;
            _tokenInfoService = tokenInfoService;
            _rolePartsValidationService = rolePartsValidationService;
            _logger = serviceProvider.ResolveLogger<CorpController>();
        }

        [HttpGet("token/{app}")]
        public async Task<IActionResult> GenerateAppPermissionToken(string app)
        {
            _logger.LogWarning($"{nameof(GenerateAppPermissionToken)} for application {RequireCorpToken.Corp}.{app}");
            
            if (!_rolePartsValidationService.IsValidApp(app).IsValid)
                return StatusCodes.Status400BadRequest.WithMessage(nameof(app));
            
            var corp = RequireCorpToken.Corp;
            
            var nextTokenVersion = await _tokenInfoService.IncreaseVersionAsync(new TokenInfo
            {
                Corp = corp,
                App = app
            });
            
            var actionResult = StatusCodes.Status200OK.WithMessage(_encryption.Encrypt(new RequestAppHeaders
            {
                Header = Constants.Headers.AppPermission,
                Corp = corp,
                App = app,
                Version = nextTokenVersion
            }.ToJson()));
            
            _logger.LogWarning($"Generated token for {RequireCorpToken.Corp}.{app} version {nextTokenVersion}");

            return actionResult;
        }
    }
}