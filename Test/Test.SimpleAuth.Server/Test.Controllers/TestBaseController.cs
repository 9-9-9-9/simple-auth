using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Controllers;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Exceptions;
using Test.SimpleAuth.Server.Support.Extensions;
using MockExtensions = Test.SimpleAuth.Server.Support.Extensions.MockExtensions;

// ReSharper disable JoinDeclarationAndInitializer

// ReSharper disable InconsistentNaming

namespace Test.SimpleAuth.Server.Test.Controllers
{
    public class TestBaseController_ErrorHandling : BaseTestServer
    {
        [Test]
        public async Task ProcedureDefaultResponseIfError()
        {
            var futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask()
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status200OK);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new EntityNotExistsException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status404NotFound);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new EntityAlreadyExistsException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status409Conflict);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new EntityAlreadyExistsException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status409Conflict);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new AccessLockedEntityException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status423Locked);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new DataVerificationMismatchException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status406NotAcceptable);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new SimpleAuthSecurityException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status403Forbidden);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new ValidationException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status400BadRequest);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new ConcurrentUpdateException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status422UnprocessableEntity);
            //
            futureSetup = M<IUserService>()
                .FSet(s => s.Setup(x =>
                        x.UpdateLockStatusAsync(It.IsAny<User>())
                    ).ReturnsTask(new SimpleAuthException(""))
                );
            await AssertHttpCode(futureSetup, StatusCodes.Status500InternalServerError);

            //
            async Task AssertHttpCode(MockExtensions.FutureSetup<IUserService> fSet, int expectHttpStatus)
            {
                var controller = SetupController(fSet.Object);
                controller.HttpContext.Request.Method = "";
                var response = await controller.LockUser(null);
                var sr = AssertE.IsAssignableFrom<ContentResult>(response);
                Assert.AreEqual(expectHttpStatus, sr.StatusCode);
            }

            UserController SetupController(IUserService userService)
            {
                var isp = Isp();
                isp.Setup(x => x.GetService(typeof(IUserService)))
                    .Returns(userService);

                isp.Setup(x => x.GetService(typeof(ILogger<UserController>)))
                    .Returns(MLog<UserController>().Object);

                var uRepo = M<IUserRepository>();
                isp.Setup(x => x.GetService(typeof(IUserRepository)))
                    .Returns(uRepo.Object);

                var controller = new UserController(isp.Object, null, null).WithHttpCtx();
                controller.HttpContext.Items[Constants.Headers.AppPermission] = new RequestAppHeaders
                {
                    Corp = "c",
                    App = "a"
                };
                return controller;
            }
        }
    }

    public class TestBaseController_Responses : BaseTestServer
    {
        [Test]
        public async Task ProcedureResponseForArrayLookUp()
        {
            await AssertResponse(Returns(new List<Role>()
            {
                new Role {RoleId = "x"}
            }), StatusCodes.Status200OK, 1);
            await AssertResponse(Returns(new List<Role>
            {
                new Role {RoleId = "x"}, new Role {RoleId = "x"}, new Role {RoleId = "x"}
            }), StatusCodes.Status200OK, 3);
            await AssertResponse(Returns(new List<Role>()), StatusCodes.Status204NoContent, 0);

            MockExtensions.FutureSetup<IRoleService> Returns(IEnumerable<Role> result)
            {
                return M<IRoleService>()
                    .FSet(s =>
                        s.Setup(x =>
                            x.SearchRoles(
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<FindOptions>()
                            )
                        ).Returns(result)
                    );
            }

            async Task AssertResponse(MockExtensions.FutureSetup<IRoleService> fSet, int expectHttpStatus,
                int expectedResponseSize)
            {
                var controller = SetupController(fSet.Object);
                controller.HttpContext.Request.Method = "";
                var response = await controller.FindRoles("term", 0, 0);
                var sr = AssertE.IsAssignableFrom<ContentResult>(response);
                Assert.AreEqual(expectHttpStatus, sr.StatusCode);
                Assert.IsTrue(sr.ContentType.Contains("application/json"));
                Assert.NotNull(sr.Content);
                var arr = JsonSerializer.Deserialize<string[]>(sr.Content);
                Assert.AreEqual(expectedResponseSize, arr.Length);

                var sizeInHeader = controller.Response.Headers["CSize1"].First();
                Assert.AreEqual(expectedResponseSize.ToString(), sizeInHeader);
            }

            RolesController SetupController(IRoleService roleService)
            {
                var isp = Isp();
                isp.Setup(x => x.GetService(typeof(IRoleService)))
                    .Returns(roleService);

                var uRepo = M<IRoleRepository>();
                isp.Setup(x => x.GetService(typeof(IRoleRepository)))
                    .Returns(uRepo.Object);

                var controller = new RolesController(isp.Object, null).WithHttpCtx();
                controller.HttpContext.Items[Constants.Headers.AppPermission] = new RequestAppHeaders
                {
                    Corp = "c",
                    App = "a"
                };
                return controller;
            }
        }

        [Test]
        public async Task ProcedureResponseForLookUp()
        {
            await AssertResponse(Returns(new RoleGroup {Name = "g1"}), StatusCodes.Status200OK);
            await AssertResponse(Returns(null), StatusCodes.Status404NotFound);

            MockExtensions.FutureSetup<IRoleGroupService> Returns(RoleGroup result)
            {
                return M<IRoleGroupService>()
                    .FSet(s =>
                        s.Setup(x =>
                            x.GetRoleGroupByName(
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>()
                            )
                        ).ReturnsAsync(result)
                    );
            }

            async Task AssertResponse(MockExtensions.FutureSetup<IRoleGroupService> fSet, int expectHttpStatus)
            {
                var controller = SetupController(fSet.Object);
                controller.HttpContext.Request.Method = "";
                var response = await controller.GetRoleGroup("term");
                var sr = AssertE.IsAssignableFrom<ContentResult>(response);
                Assert.AreEqual(expectHttpStatus, sr.StatusCode);
                if (expectHttpStatus == 200)
                {
                    Assert.IsTrue(sr.ContentType.Contains("application/json"));
                    Assert.NotNull(sr.Content);
                }
            }

            RoleGroupsController SetupController(IRoleGroupService roleService)
            {
                var isp = Isp();
                isp.Setup(x => x.GetService(typeof(IRoleGroupService)))
                    .Returns(roleService);

                var uRepo = M<IRoleGroupRepository>();
                isp.Setup(x => x.GetService(typeof(IRoleGroupRepository)))
                    .Returns(uRepo.Object);

                var controller = new RoleGroupsController(isp.Object, null).WithHttpCtx();
                controller.HttpContext.Items[Constants.Headers.AppPermission] = new RequestAppHeaders
                {
                    Corp = "c",
                    App = "a"
                };
                return controller;
            }
        }
    }
}