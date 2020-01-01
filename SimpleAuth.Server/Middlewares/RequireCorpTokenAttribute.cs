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
    public class RequireCorpTokenAttribute : BaseActionFilterAttribute
    {
        protected override void ComputeAndModifyIfNeeded(ActionExecutingContext actionExecutingContext)
        {
            var token = actionExecutingContext.HttpContext.Request.Headers[Constants.Headers.CorpPermission];

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
                    var obj = decryptedToken.FromJson<RequireCorpToken>();
                    if (obj == null || obj.Corp.IsBlank() || obj.Version == 0)
                    {
                        actionExecutingContext.Result = StatusCodes.Status412PreconditionFailed.WithEmpty();
                        return;
                    }
                    
                    if (obj.Header.IsBlank() || obj.Header != Constants.Headers.CorpPermission)
                    {
                        actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithMessage(nameof(RequireCorpToken.Header));
                        return;
                    }

                    var tokenInfoService = actionExecutingContext.ResolveService<ITokenInfoService>();
                    var currentTokenVersion = tokenInfoService.GetCurrentVersion(new TokenInfo
                    {
                        Corp = obj.Corp,
                        App = string.Empty
                    });
                    if (obj.Version != currentTokenVersion)
                    {
                        actionExecutingContext.Result =
                            StatusCodes.Status426UpgradeRequired.WithMessage(
                                $"Mis-match token {nameof(TokenInfo.Version)}");
                        return;
                    }
                    
                    actionExecutingContext.HttpContext.Items[Constants.Headers.CorpPermission] = obj;
                }
            }
            catch
            {
                actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithEmpty();
            }
        }
    }
}