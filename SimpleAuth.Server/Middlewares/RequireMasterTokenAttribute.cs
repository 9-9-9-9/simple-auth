using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Server.Extensions;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared;

namespace SimpleAuth.Server.Middlewares
{
    /// <summary>
    /// Controllers/Actions which has this attribute, requires master token in header named x-master-token
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireMasterTokenAttribute : BaseActionFilterAttribute
    {
        /// <summary>
        /// Do the business
        /// </summary>
        protected override void ComputeAndModifyIfNeeded(ActionExecutingContext actionExecutingContext)
        {
            var expectedToken =
                actionExecutingContext
                    .ResolveService<SecretConstants>()
                    .MasterTokenValue;
            
            if (expectedToken.IsBlank())
                throw new InvalidDataException($"{nameof(SecretConstants.MasterTokenValue)} had not been setup yet");
            
            var token = actionExecutingContext.HttpContext.Request.Headers[Constants.Headers.MasterToken];

            var encryptionService = actionExecutingContext.ResolveService<IEncryptionService>();
            try
            {
                var decryptedToken = encryptionService.Decrypt(token);
                if (decryptedToken.IsBlank() || !expectedToken.Equals(decryptedToken))
                {
                    actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithEmpty();
                }
            }
            catch
            {
                actionExecutingContext.Result = StatusCodes.Status403Forbidden.WithEmpty();
            }
        }
    }
}