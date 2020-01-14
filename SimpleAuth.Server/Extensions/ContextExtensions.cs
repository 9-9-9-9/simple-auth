using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Server.Extensions
{
    public static class ContextExtensions
    {
        public static T ResolveService<T>(this ActionExecutingContext actionExecutingContext)
        {
            return actionExecutingContext.HttpContext.ResolveService<T>();
        }
        
        public static ILogger<T> ResolveLogger<T>(this ActionExecutingContext actionExecutingContext)
        {
            return actionExecutingContext.HttpContext.ResolveService<ILogger<T>>();
        }

        public static T ResolveService<T>(this HttpContext httpContext)
        {
            return httpContext.RequestServices.CreateScope().ServiceProvider.GetService<T>();
        }

        public static IActionResult WithEmpty(this int statusCode)
        {
            return statusCode.With(string.Empty);
        }
        
        public static IActionResult WithMessage(this int statusCode, string message)
        {
            return statusCode.With(message, "text/plain; charset=UTF-8");
        }

        public static IActionResult With(this int statusCode, object content, string contentType = null)
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = content?.ToString(),
                ContentType = contentType,
            };
        }

        public static IActionResult WithJson(this int statusCode, object content)
        {
            return statusCode.With(content?.ToJson(), "application/json; charset=UTF-8");
        }

        public static int OrLocked(this int statusCode, ILockable entity)
        {
            return entity.Locked ? StatusCodes.Status423Locked : statusCode;
        }
    }
}