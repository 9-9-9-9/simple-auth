using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Server.Extensions;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Domains;

namespace SimpleAuth.Server.Middlewares
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireCorpTokenAttribute : BaseActionFilterAttribute
    {
        protected override void ComputeAndModifyIfNeeded(ActionExecutingContext actionExecutingContext)
        {
            var logger = actionExecutingContext.ResolveLogger<RequireCorpTokenAttribute>();

            var token = actionExecutingContext.HttpContext.Request.Headers[Constants.Headers.CorpPermission];

            var encryptionService = actionExecutingContext.ResolveService<IEncryptionService>();

            if (!encryptionService.TryDecrypt(token, out var decryptedToken) || decryptedToken.IsBlank())
            {
                logger.LogInformation("Decrypt failure");
                actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithEmpty();
            }
            else
            {
                var requireCorpToken = decryptedToken.FromJson<RequireCorpToken>();

                if (requireCorpToken == null || requireCorpToken.Corp.IsBlank() || requireCorpToken.Version == 0)
                {
                    logger.LogInformation($"{nameof(RequireCorpToken)} has invalid content");
                    actionExecutingContext.Result = StatusCodes.Status412PreconditionFailed.WithEmpty();
                    return;
                }

                if (requireCorpToken.Header.IsBlank() || requireCorpToken.Header != Constants.Headers.CorpPermission)
                {
                    logger.LogInformation($"{nameof(RequireCorpToken)} has invalid content");
                    actionExecutingContext.Result =
                        StatusCodes.Status403Forbidden.WithMessage(nameof(RequireCorpToken.Header));
                    return;
                }

                var tokenInfoService = actionExecutingContext.ResolveService<ITokenInfoService>();
                var currentTokenVersion = tokenInfoService.GetCurrentVersionAsync(new TokenInfo
                {
                    Corp = requireCorpToken.Corp,
                    App = string.Empty
                }).Result;
                if (requireCorpToken.Version != currentTokenVersion)
                {
                    logger.LogError(
                        $"Client using an out dated token version {requireCorpToken.Version}, current version is {currentTokenVersion}");
                    actionExecutingContext.Result =
                        StatusCodes.Status426UpgradeRequired.WithMessage(
                            $"Mis-match token {nameof(TokenInfo.Version)}, expected {currentTokenVersion} but {requireCorpToken.Version}");
                    return;
                }

                actionExecutingContext.HttpContext.Items[Constants.Headers.CorpPermission] = requireCorpToken;
                logger.LogInformation($"Access granted for {nameof(RequireCorpToken)}");
                
                actionExecutingContext.HttpContext.Response.Headers.Add(Constants.Headers.SourceCorp, requireCorpToken.Corp);
            }
        }
    }
}