using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

public class SaAuthorizationAsyncActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller;
        var methodInfoOfAction = context.ActionDescriptor.GetType().GetProperty("MethodInfo")?.GetValue(context.ActionDescriptor);
        
        await next();
        // Nothing to do
    }
}