using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared;
using Test.Shared.Extensions;

namespace Test.SimpleAuth.Server.Test.Middlewares
{
    public class TestRequireMasterTokenAttribute : BaseTestAttribute
    {
        const string MasterToken = "This is secret master token";

        [TestCase(MasterToken, true)]
        [TestCase("This is secret master toke", false)]
        [TestCase("this is secret master token", false)]
        [TestCase("This is secret master toke ", false)]
        [TestCase("This is secret master token ", false)]
        [TestCase(" This is secret master token", false)]
        [TestCase(" This is secret master token ", false)]
        [TestCase("This is secret master token    ", false)]
        [TestCase("    This is secret master token", false)]
        [TestCase("    This is secret master token    ", false)]
        [TestCase("{'Corp': 'c', 'App': 'a'}", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void RequireMasterToken(string input, bool valid)
        {
            var ctx = M<HttpContext>();
            var req = M<HttpRequest>();
            var res = M<HttpResponse>();
            var svc = M<IServiceProvider>();
            var scp = M<IServiceScope>();
            var fac = M<IServiceScopeFactory>();

            svc.Setup(x => x.GetService(typeof(IEncryptionService))).Returns(new DummyEncryptionService());
            svc.Setup(x => x.GetService(typeof(SecretConstants))).Returns(new SecretConstants(MasterToken));
            ctx.SetupGet(x => x.RequestServices).Returns(svc.Object);
            svc.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(fac.Object);
            fac.Setup(x => x.CreateScope()).Returns(scp.Object);
            scp.SetupGet(x => x.ServiceProvider).Returns(svc.Object);
            ctx.SetupGet(x => x.Request).Returns(req.Object);
            ctx.SetupGet(x => x.Response).Returns(res.Object);
            req.SetupGet(x => x.Headers).Returns(
                new HeaderDictionary(new Dictionary<string, StringValues>()
                {
                    {Constants.Headers.MasterToken, new StringValues(input)}
                })
            );

            var attr = new RequireMasterTokenAttribute();
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

            if (valid)
            {
                Assert.IsNull(actResult);
            }
            else
            {
                var ar = AssertE.IsAssignableFrom<ContentResult>(actResult);
                Assert.AreEqual(StatusCodes.Status403Forbidden, ar.StatusCode);
            }
        }
    }
}