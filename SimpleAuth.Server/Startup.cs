using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SimpleAuth.Core.DependencyInjection;
using SimpleAuth.Core.Utils;
using SimpleAuth.Repositories;
using SimpleAuth.Server;
using SimpleAuth.Server.Models;
using SimpleAuth.Server.Swagger;
using Constants = SimpleAuth.Shared.Constants;
#pragma warning disable 1591

namespace SimpleAuthServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var secretSection = Configuration.GetSection(Constants.Encryption.Section);
            services.AddSingleton(new EncryptionUtils.EncryptionKeyPair
            {
                PublicKey = secretSection[Constants.Encryption.PublicKeyName],
                PrivateKey = secretSection[Constants.Encryption.PrivateKeyName]
            });

            services.AddSingleton(new SecretConstants(secretSection[Constants.Encryption.MasterTokenKey]));

            services.RegisterModules<SimpleAuth.Shared.ProjectRegistrableModules>();
            services.RegisterModules<SimpleAuth.Services.ProjectRegistrableModules>();
            services.RegisterModules<SimpleAuth.Postgres.ProjectRegistrableModules>();
            services.AddTransient<SimpleAuthDbContext, DbContext>();

            services.RegisterModules<SimpleAuth.Server.Services.ServiceModules>();

            //
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "SimpleAuth full API", 
                    Description = "<a href='https://github.com/9-9-9-9/simple-auth'>Watch me on GitHub</a>"
                });
                
                c.OperationFilter<AddRequiredHeaderParameter>();
                
                c.DocumentFilter<TagDescriptionsDocumentFilter>();
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //TODO HTTPS
            //app.UseHttpsRedirection();
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SimpleAuth full API");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}