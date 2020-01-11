using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
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
            var token = actionExecutingContext.HttpContext.Request.Headers[Constants.Headers.AppPermission];

            var encryptionService = actionExecutingContext.ResolveService<IEncryptionService>();
            try
            {
                var decryptedToken = encryptionService.Decrypt(token);
                if (decryptedToken.IsBlank())
                {
                    actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithEmpty();
                }
                else
                {
                    var obj = decryptedToken.FromJson<RequestAppHeaders>();
                    if (obj == null || obj.Corp.IsBlank() || obj.App.IsBlank())
                    {
                        actionExecutingContext.Result = StatusCodes.Status412PreconditionFailed.WithEmpty();
                        return;
                    }

                    if (obj.Header.IsBlank() || obj.Header != Constants.Headers.AppPermission)
                    {
                        actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithMessage(nameof(RequestAppHeaders.Header));
                        return;
                    }

                    var tokenInfoService = actionExecutingContext.ResolveService<ITokenInfoService>();
                    var currentTokenVersion = tokenInfoService.GetCurrentVersionAsync(new TokenInfo
                    {
                        Corp = obj.Corp,
                        App = obj.App
                    }).Result;
                    if (obj.Version != currentTokenVersion)
                    {
                        actionExecutingContext.Result =
                            StatusCodes.Status426UpgradeRequired.WithMessage(
                                $"Mis-match token {nameof(TokenInfo.Version)}, expected {obj.Version}, current {currentTokenVersion}");
                        return;
                    }
                        
                    actionExecutingContext.HttpContext.Items[Constants.Headers.AppPermission] = obj;
                }
            }
            catch
            {
                actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithEmpty();
            }
        }
    }
}