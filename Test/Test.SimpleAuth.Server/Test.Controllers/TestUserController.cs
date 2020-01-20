using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Controllers;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Exceptions;
using Test.Shared.Extensions;
using Test.Shared.Utils;
using Test.SimpleAuth.Server.Support.Extensions;
using MockExtensions = Test.SimpleAuth.Server.Support.Extensions.MockExtensions;

// ReSharper disable JoinDeclarationAndInitializer

// ReSharper disable InconsistentNaming

namespace Test.SimpleAuth.Server.Test.Controllers
{
    public class TestUserController_LockUser : BaseTestController<IUserService, User, IUserRepository,
        global::SimpleAuth.Services.Entities.User, UserController>
    {
        [Test]
        public async Task LockUser()
        {
            var futureSetup = MSvc()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask()
                );
            await LockUser(futureSetup, 200);
            //
            futureSetup = MSvc()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new EntityNotExistsException(""))
                );
            await LockUser(futureSetup, 404);

            //
            async Task LockUser(MockExtensions.FutureSetup<IUserService> fSet, int expectHttpStatus)
            {
                var controller = SetupController(fSet.Object);
                controller.HttpContext.Request.Method = "";
                var response = await controller.LockUser(null);
                var sr = AssertE.IsAssignableFrom<ContentResult>(response);
                Assert.AreEqual(expectHttpStatus, sr.StatusCode);
            }
        }

        protected UserController SetupController(IUserService userService)
        {
            var isp = MoqU.OfServiceProviderFor<UserController>()
                .WithIn(userService)
                .WithIn(MRepo().Object);

            var controller = new UserController(isp.Object, null, null).WithHttpCtx();
            controller.HttpContext.Items[Constants.Headers.AppPermission] = new RequestAppHeaders
            {
                Corp = "c",
                App = "a"
            };
            return controller;
        }

        protected override UserController SetupController()
        {
            return null;
        }
    }
}