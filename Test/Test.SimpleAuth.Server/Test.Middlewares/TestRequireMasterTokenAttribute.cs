using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared;
using Test.Shared.Extensions;
using Test.Shared.Utils;
using Test.SimpleAuth.Server.Support.Extensions;

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
            var ctx = Mu.OfHttpContext(out var svc, out _, out _, out _, out _,
                requestHeaders: new HeaderDictionary(
                    new Dictionary<string, StringValues>()
                    {
                        {Constants.Headers.MasterToken, new StringValues(input)}
                    }));

            svc
                .WithIn<IEncryptionService>(new DummyEncryptionService())
                .WithIn(new SecretConstants(MasterToken));

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