using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Test.Shared.Extensions
{
    public static class ControllerExtensions
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