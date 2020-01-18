using System.Collections.Generic;
using System.Reflection;
using Microsoft.OpenApi.Models;
using SimpleAuth.Server.Middlewares;
using SimpleAuth.Shared;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SimpleAuth.Server.Swagger
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            if (context.MethodInfo.DeclaringType?.GetCustomAttribute<RequireAppTokenAttribute>() != null)
                operation.Parameters.AddHeaderRequirement(
                    Constants.Headers.AppPermission,
                    "Token contains Corp and App information, indicate target of requesting"
                );
            else if (context.MethodInfo.DeclaringType?.GetCustomAttribute<RequireCorpTokenAttribute>() != null)
                operation.Parameters.AddHeaderRequirement(
                    Constants.Headers.CorpPermission,
                    "Token contains Corp information, indicate target of requesting"
                );
            else if (context.MethodInfo.DeclaringType?.GetCustomAttribute<RequireMasterTokenAttribute>() != null)
                operation.Parameters.AddHeaderRequirement(
                    Constants.Headers.MasterToken,
                    "Token contains master value, indicate requester is administrator"
                );
        }
    }

    internal static class OperationFilterParameterExtensions
    {
        public static void AddHeaderRequirement(this IList<OpenApiParameter> parameters, string name, string desc)
        {
            parameters.Add(new OpenApiParameter
            {
                Name = name,
                In = ParameterLocation.Header,
                Required = true,
                Description = desc
            });
        }
    }
}