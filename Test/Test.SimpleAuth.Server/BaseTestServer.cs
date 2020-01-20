using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Controllers;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared.Domains;
using Test.Shared.Utils;

namespace Test.SimpleAuth.Server
{
    public abstract class BaseTestServer
    {
        protected Mock<T> M<T>(MockBehavior mockBehavior = MockBehavior.Strict) where T : class
        {
            return Mu.Of<T>(mockBehavior);
        }
    }

    public abstract class BaseTestAttribute : BaseTestServer
    {
        
    }

    public abstract class BaseTestController<TSvc, TDomain, TRepo, TEntity, TController> : BaseTestServer
        where TSvc : class, IDomainService
        where TDomain : BaseDomain
        where TRepo : class, IRepository<TEntity>
        where TEntity : BaseEntity
        where TController : BaseController<TSvc, TRepo, TEntity>
    {
        // ReSharper disable once UnassignedReadonlyField
        protected readonly TController Controller;

        protected BaseTestController()
        {
            Controller = SetupController();
        }

        protected abstract TController SetupController();
        
        protected Mock<TSvc> MSvc(MockBehavior mockBehavior = MockBehavior.Strict) => M<TSvc>(mockBehavior);
        protected Mock<TRepo> MRepo(MockBehavior mockBehavior = MockBehavior.Strict) => M<TRepo>(mockBehavior);
    }

    public static class BaseTestControllerExtensions
    {
        public static TController WithCtx<TController>(this TController controller) where TController : ControllerBase
        {
            controller.ControllerContext = new ControllerContext();
            return controller;
        }
        
        public static TController WithHttpCtx<TController>(this TController controller) where TController : ControllerBase
        {
            if (controller.ControllerContext == null)
                controller = controller.WithCtx();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            return controller;
        }
        
        public static TController WithCtx<TController>(this TController controller, Mock<HttpContext> httpCtx) where TController : ControllerBase
        {
            if (controller.ControllerContext == null)
                controller = controller.WithCtx();
            controller.ControllerContext.HttpContext = httpCtx.Object;
            return controller;
        }
    }
}