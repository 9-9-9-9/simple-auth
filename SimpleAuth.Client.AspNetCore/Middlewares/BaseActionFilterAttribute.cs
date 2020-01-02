using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SimpleAuth.Client.AspNetCore.Middlewares
{
    public abstract class BaseActionFilterAttribute : ActionFilterAttribute

    {
        protected abstract void ComputeAndModifyIfNeeded(ActionExecutingContext actionExecutingContext);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ComputeAndModifyIfNeeded(context);
            base.OnActionExecuting(context);
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ComputeAndModifyIfNeeded(context);
            return base.OnActionExecutionAsync(context, next);
        }
    }
}