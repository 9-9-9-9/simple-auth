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
using Test.SimpleAuth.Server.Support.Extensions;

namespace Test.SimpleAuth.Server.Test.Middlewares
{
    public class TestRequireCorpTokenAttribute : BaseTestAttribute
    {
        [TestCase("{\"Corp\": \"c\", \"Version\": 1, \"Header\": \"x-corp-token\"}", true, 0)]
        [TestCase("{\"Corp\": \"c\", \"Version\": 1, \"Header\": \"x-Corp-token\"}", false, StatusCodes.Status403Forbidden)]
        [TestCase("{\"Corp\": \"\", \"Version\": 1, \"Header\": \"x-corp-token\"}", false, StatusCodes.Status412PreconditionFailed)]
        [TestCase("{\"Corp\": \"c\", \"Version\": 0, \"Header\": \"x-corp-token\"}", false, StatusCodes.Status412PreconditionFailed)]
        [TestCase("{\"Corp\": \"c\", \"Version\": 2, \"Header\": \"x-corp-token\"}", false, StatusCodes.Status426UpgradeRequired)]
        [TestCase("", false, StatusCodes.Status403Forbidden)]
        [TestCase(null, false, StatusCodes.Status403Forbidden)]
        public void RequireCorpToken(string input, bool valid, int expectedStatusCde)
        {
            var ctx = M<HttpContext>();
            var req = M<HttpRequest>();
            var res = M<HttpResponse>();
            var svc = M<IServiceProvider>();
            var scp = M<IServiceScope>();
            var fac = M<IServiceScopeFactory>();
            var tkn = M<ITokenInfoService>();
            var log = MLog<RequireCorpTokenAttribute>();

            svc.Setup(x => x.GetService(typeof(ILogger<RequireCorpTokenAttribute>))).Returns(log.Object);
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
                    {Constants.Headers.CorpPermission, new StringValues(input)}
                })
            );
            res.SetupGet(x => x.Headers).Returns(
                new HeaderDictionary(new Dictionary<string, StringValues>()
                {
                })
            );
            ctx.SetupGet(x => x.Items).Returns(new Dictionary<object, object>());

            var attr = new RequireCorpTokenAttribute();
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

            attr.OnActionExecuting(actionExecutingContext);

            var actResult = actionExecutingContext.Result;
            var items = actionExecutingContext.HttpContext.Items;

            if (valid)
            {
                Assert.IsTrue(items.Count == 1);
                Assert.IsTrue(items.ContainsKey(Constants.Headers.CorpPermission));
                var rah = AssertE.IsAssignableFrom<RequireCorpToken>(items[Constants.Headers.CorpPermission]);
                Assert.IsFalse(rah.Corp.IsBlank());
                Assert.IsNull(actResult);
            }
            else
            {
                var ar = AssertE.IsAssignableFrom<ContentResult>(actResult);
                Assert.AreEqual(expectedStatusCde, ar.StatusCode);
            }
        }
    }
}