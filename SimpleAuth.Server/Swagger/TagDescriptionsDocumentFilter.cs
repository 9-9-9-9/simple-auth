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
                new OpenApiTag {Name = "Google", Description = "Reserved Endpoint for serving requests relate to Google, such as sign-in using OAuth token"},
                new OpenApiTag {Name = "RoleGroups"},
                new OpenApiTag {Name = "Roles"},
                new OpenApiTag {Name = "Administration", Description = "Master controller for administration. By providing a master token as `x-master-token` header, requester can access ultimate features"},
                new OpenApiTag {Name = "Corp", Description = "Controller for managing Corp. By providing a corp-level token as `x-corp-token` header, requester can access features"},
            };
        }
    }
}