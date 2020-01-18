using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SimpleAuth.Server.Swagger
{
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    /// Being used to sorting the order of tags
    /// </summary>
    public class TagDescriptionsDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Apply tag with customized order
        /// </summary>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = new List<OpenApiTag>
            {
                new OpenApiTag {Name = "User"},
                new OpenApiTag {Name = "Google"},
                new OpenApiTag {Name = "RoleGroups"},
                new OpenApiTag {Name = "Roles"},
                new OpenApiTag {Name = "Administration"},
                new OpenApiTag {Name = "Corp"},
            };
        }
    }
}