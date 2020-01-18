using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SimpleAuth.Server.Swagger
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TagDescriptionsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = new List<OpenApiTag>
            {
                new OpenApiTag {Name = "User"},
                new OpenApiTag {Name = "Google", Description = "Authentication using Google OAuth Token"},
                new OpenApiTag {Name = "RoleGroups"},
                new OpenApiTag {Name = "Roles"},
                new OpenApiTag {Name = "Administration", Description = "Manage api tokens"},
                new OpenApiTag {Name = "Corp", Description = "Manage api tokens"},
            };
        }
    }
}