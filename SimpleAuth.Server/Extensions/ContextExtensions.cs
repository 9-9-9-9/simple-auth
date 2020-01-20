using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimpleAuth.Server.Extensions
{
    /// <summary>
    /// Extensions for context-related
    /// </summary>
    public static class ContextExtensions
    {
        /// <summary>
        /// Provide a minimum typing to resolve a service
        /// </summary>
        public static T ResolveService<T>(this ActionExecutingContext actionExecutingContext)
        {
            return actionExecutingContext.HttpContext.ResolveService<T>();
        }
        
        /// <summary>
        /// Provide a minimum typing to resolve an ILogger class
        /// </summary>
        public static ILogger<T> ResolveLogger<T>(this ActionExecutingContext actionExecutingContext)
        {
            return actionExecutingContext.HttpContext.ResolveService<ILogger<T>>();
        }
        
        /// <summary>
        /// Provide a minimum typing to resolve an ILogger class
        /// </summary>
        public static ILogger<T> ResolveLogger<T>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ILogger<T>>();
        }

        /// <summary>
        /// Provide a minimum typing to resolve a service
        /// </summary>
        public static T ResolveService<T>(this HttpContext httpContext)
        {
            return httpContext.RequestServices.CreateScope().ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// Procedure an action result without any content
        /// </summary>
        /// <param name="statusCode">Response HTTP status code</param>
        public static IActionResult WithEmpty(this int statusCode)
        {
            return statusCode.With(string.Empty);
        }
        
        /// <summary>
        /// Procedure an action result with response body of text/plain content type 
        /// </summary>
        /// <param name="statusCode">Response HTTP status code</param>
        /// <param name="message">Response body</param>
        public static IActionResult WithMessage(this int statusCode, string message)
        {
            return statusCode.With(message, "text/plain; charset=UTF-8");
        }

        /// <summary>
        /// Procedure an action result with custom response HTTP status code, content type and body
        /// </summary>
        /// <param name="statusCode">Response HTTP status code</param>
        /// <param name="content">Response body</param>
        /// <param name="contentType">Content type</param>
        /// <returns></returns>
        public static IActionResult With(this int statusCode, object content, string contentType = null)
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = content?.ToString(),
                ContentType = contentType,
            };
        }

        /// <summary>
        /// Procedure an action result with response body of application/json charset UTF-8 content type 
        /// </summary>
        /// <param name="statusCode">Response HTTP status code</param>
        /// <param name="content">Response body</param>
        public static IActionResult WithJson(this int statusCode, object content)
        {
            return statusCode.With(content?.ToJson(), "application/json; charset=UTF-8");
        }
    }
}