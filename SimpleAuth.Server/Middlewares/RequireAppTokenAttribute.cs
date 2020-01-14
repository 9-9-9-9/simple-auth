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
    public class RequireAppTokenAttribute : BaseActionFilterAttribute
    {
        protected override void ComputeAndModifyIfNeeded(ActionExecutingContext actionExecutingContext)
        {
            var logger = actionExecutingContext.ResolveLogger<RequireAppTokenAttribute>();

            var token = actionExecutingContext.HttpContext.Request.Headers[Constants.Headers.AppPermission];

            var encryptionService = actionExecutingContext.ResolveService<IEncryptionService>();

            if (!encryptionService.TryDecrypt(token, out var decryptedToken) || decryptedToken.IsBlank())
            {
                logger.LogInformation("Decrypt failure");
                actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithEmpty();
            }
            else
            {
                RequestAppHeaders requestAppHeaders;
                try
                {
                    requestAppHeaders = decryptedToken.FromJson<RequestAppHeaders>();
                }
                catch (Exception ex)
                {
                    logger.LogError($"Deserialize {nameof(RequestAppHeaders)} failure", ex);
                    actionExecutingContext.Result = StatusCodes.Status500InternalServerError.WithEmpty();
                    return;
                }

                if (requestAppHeaders == null || requestAppHeaders.Corp.IsBlank() || requestAppHeaders.App.IsBlank())
                {
                    logger.LogInformation($"{nameof(RequestAppHeaders)} has invalid content");
                    actionExecutingContext.Result = StatusCodes.Status412PreconditionFailed.WithEmpty();
                    return;
                }

                if (requestAppHeaders.Header.IsBlank() || requestAppHeaders.Header != Constants.Headers.AppPermission)
                {
                    logger.LogInformation($"{nameof(RequestAppHeaders)} has invalid content");
                    actionExecutingContext.Result =
                        StatusCodes.Status403Forbidden.WithMessage(nameof(RequestAppHeaders.Header));
                    return;
                }

                var tokenInfoService = actionExecutingContext.ResolveService<ITokenInfoService>();
                var currentTokenVersion = tokenInfoService.GetCurrentVersionAsync(new TokenInfo
                {
                    Corp = requestAppHeaders.Corp,
                    App = requestAppHeaders.App
                }).Result;
                if (requestAppHeaders.Version != currentTokenVersion)
                {
                    logger.LogError(
                        $"Client using an out dated token version {requestAppHeaders.Version}, current version is {currentTokenVersion}");
                    actionExecutingContext.Result =
                        StatusCodes.Status426UpgradeRequired.WithMessage(
                            $"Mis-match token {nameof(TokenInfo.Version)}, expected {currentTokenVersion} but {requestAppHeaders.Version}");
                    return;
                }

                actionExecutingContext.HttpContext.Items[Constants.Headers.AppPermission] = requestAppHeaders;
                logger.LogInformation($"Access granted for {nameof(RequestAppHeaders)}");
            }
        }
    }
}