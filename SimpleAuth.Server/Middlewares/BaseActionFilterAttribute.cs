using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SimpleAuth.Server.Middlewares
{
    /// <summary>
    /// Base method, always call ComputeAndModifyIfNeeded for both sync and async methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class BaseActionFilterAttribute : ActionFilterAttribute

    {
        /// <summary>
        /// Do the business here
        /// </summary>
        protected abstract void ComputeAndModifyIfNeeded(ActionExecutingContext actionExecutingContext);

        /// <summary>
        /// Modified action executing pipeline to call ComputeAndModifyIfNeeded
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ComputeAndModifyIfNeeded(context);
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Modified action executing pipeline to call ComputeAndModifyIfNeeded
        /// </summary>
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ComputeAndModifyIfNeeded(context);
            return base.OnActionExecutionAsync(context, next);
        }
    }
}