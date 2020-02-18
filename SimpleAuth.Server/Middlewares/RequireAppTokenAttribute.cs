using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Controllers/Actions which has this attribute, requires App-level token in header named x-app-token
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireAppTokenAttribute : BaseActionFilterAttribute
    {
        private readonly bool _allowReadOnly;

        /// <inheritdoc />
        public RequireAppTokenAttribute(bool allowReadOnly = false)
        {
            _allowReadOnly = allowReadOnly;
        }

        /// <summary>
        /// Do the business
        /// </summary>
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
                var requestAppHeaders = decryptedToken.FromJson<RequestAppHeaders>();

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
                            $"Mis-match token {nameof(TokenInfo.Version)}, expected {currentTokenVersion} but {requestAppHeaders.Version}"
                        );
                    return;
                }

                if (!_allowReadOnly && requestAppHeaders.ReadOnly)
                {
                    logger.LogError(
                        $"Client using a read-only token version {requestAppHeaders.Version}, corp {requestAppHeaders.Corp}, app {requestAppHeaders.App}");
                    actionExecutingContext.Result =
                        StatusCodes.Status426UpgradeRequired.WithMessage(
                            "Expected Non read-only token"
                        );
                    return;
                }

                actionExecutingContext.HttpContext.Items[Constants.Headers.AppPermission] = requestAppHeaders;
                logger.LogInformation($"Access granted for {nameof(RequestAppHeaders)}");

                actionExecutingContext.HttpContext.Response.Headers.TryAdd(Constants.Headers.SourceCorp,
                    requestAppHeaders.Corp);
                actionExecutingContext.HttpContext.Response.Headers.TryAdd(Constants.Headers.SourceApp,
                    requestAppHeaders.App);
            }
        }
    }
}