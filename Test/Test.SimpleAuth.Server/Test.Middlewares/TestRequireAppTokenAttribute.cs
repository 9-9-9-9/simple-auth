using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Domains;
using Test.Shared.Extensions;
using Test.SimpleAuth.Server.Support.Extensions;

namespace Test.SimpleAuth.Server.Test.Middlewares
{
    public class TestRequireAppTokenAttribute : BaseTestAttribute
    {
        [TestCase("{\"Corp\": \"c\", \"App\": \"a\", \"Version\": 1, \"Header\": \"x-app-token\"}", true, 0)]
        [TestCase("{\"Corp\": \"c\", \"App\": \"a\", \"Version\": 1, \"Header\": \"x-App-token\"}", false,
            StatusCodes.Status403Forbidden)]
        [TestCase("{\"Corp\": \"c\", \"App\": \"a\", \"Version\": 0, \"Header\": \"x-app-token\"}", false,
            StatusCodes.Status426UpgradeRequired)]
        [TestCase("{\"Corp\": \"c\", \"App\": \"a\", \"Version\": 2, \"Header\": \"x-app-token\"}", false,
            StatusCodes.Status426UpgradeRequired)]
        [TestCase("{'Corp': 'c', 'App': 'a', 'Version': 1, 'Header': 'x-app-token'}", false, 0, true)]
        [TestCase("{\"corp\": \"c\", \"App\": \"a\", \"Version\": 1, \"Header\": \"x-app-token\"}", false,
            StatusCodes.Status412PreconditionFailed)]
        [TestCase("{\"Corp\": \"c\", \"App\": \"\", \"Version\": 1, \"Header\": \"x-app-token\"}", false,
            StatusCodes.Status412PreconditionFailed)]
        [TestCase("{\"Corp\": \"\", \"App\": \"a\", \"Version\": 1, \"Header\": \"x-app-token\"}", false,
            StatusCodes.Status412PreconditionFailed)]
        [TestCase("{\"Corp\": \"\", \"App\": \"\", \"Version\": 1, \"Header\": \"x-app-token\"}", false,
            StatusCodes.Status412PreconditionFailed)]
        [TestCase("", false, StatusCodes.Status403Forbidden)]
        [TestCase(null, false, StatusCodes.Status403Forbidden)]
        public void RequireAppToken(string input, bool valid, int expectedStatusCde, bool expectedError = false)
        {
            var ctx = M<HttpContext>();
            var req = M<HttpRequest>();
            var res = M<HttpResponse>();
            var svc = M<IServiceProvider>();
            var scp = M<IServiceScope>();
            var fac = M<IServiceScopeFactory>();
            var tkn = M<ITokenInfoService>();
            var log = MLog<RequireAppTokenAttribute>();

            svc.Setup(x => x.GetService(typeof(ILogger<RequireAppTokenAttribute>))).Returns(log.Object);
            svc.Setup(x => x.GetService(typeof(IEncryptionService))).Returns(new DummyEncryptionService());
            tkn.Setup(x => x.GetCurrentVersionAsync(It.IsAny<TokenInfo>())).ReturnsAsync(1);
            svc.Setup(x => x.GetService(typeof(ITokenInfoService))).Returns(tkn.Object);
            ctx.SetupGet(x => x.RequestServices).Returns(svc.Object);
            svc.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(fac.Object);
            fac.Setup(x => x.CreateScope()).Returns(scp.Object);
            scp.SetupGet(x => x.ServiceProvider).Returns(svc.Object);
            ctx.SetupGet(x => x.Request).Returns(req.Object);
            ctx.SetupGet(x => x.Response).Returns(res.Object);
            req.SetupGet(x => x.Headers).Returns(
                new HeaderDictionary(new Dictionary<string, StringValues>()
                {
                    {Constants.Headers.AppPermission, new StringValues(input)}
                })
            );
            res.SetupGet(x => x.Headers).Returns(
                new HeaderDictionary(new Dictionary<string, StringValues>()
                {
                })
            );
            ctx.SetupGet(x => x.Items).Returns(new Dictionary<object, object>());

            var attr = new RequireAppTokenAttribute();
            var actionContext = new ActionContext(
                ctx.Object,
                Mock.Of<RouteData>(),
                Mock.Of<ActionDescriptor>(),
                new ModelStateDictionary()
            );
            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                Mock.Of<Controller>()
            );

            try
            {
                attr.OnActionExecuting(actionExecutingContext);
                if (expectedError)
                    Assert.Fail("Expect error");
            }
            catch
            {
                if (expectedError)
                    // pass
                    return;

                throw;
            }

            var actResult = actionExecutingContext.Result;
            var items = actionExecutingContext.HttpContext.Items;

            if (valid)
            {
                Assert.IsTrue(items.Count == 1);
                Assert.IsTrue(items.ContainsKey(Constants.Headers.AppPermission));
                var rah = AssertE.IsAssignableFrom<RequestAppHeaders>(items[Constants.Headers.AppPermission]);
                Assert.IsFalse(rah.Corp.IsBlank());
                Assert.IsFalse(rah.App.IsBlank());
                Assert.IsNull(actResult);
            }
            else
            {
                var ar = AssertE.IsAssignableFrom<ContentResult>(actResult);
                Assert.AreEqual(expectedStatusCde, ar.StatusCode);
            }
        }
    }

    public class DummyEncryptionService : IEncryptionService
    {
        public string Encrypt(string plainTextData)
        {
            return plainTextData;
        }

        public string Decrypt(string encryptedData)
        {
            return encryptedData;
        }

        public bool TryEncrypt(string plainTextData, out string encryptedData)
        {
            encryptedData = plainTextData;
            return true;
        }

        public bool TryDecrypt(string encryptedData, out string decryptedData)
        {
            decryptedData = encryptedData;
            return true;
        }
    }
}