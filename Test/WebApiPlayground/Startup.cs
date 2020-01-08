using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleAuth.Client;
using SimpleAuth.Client.AspNetCore.Middlewares;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.DependencyInjection;

namespace WebApiPlayground
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

            services.UseSimpleAuthDefaultServices(new SimpleAuthSettings
            {
                SimpleAuthServerUrl = "http://standingtrust.com",
                CorpToken = "",
                AppToken = "",
                Tenant = "t"
            });
            
            services
                .AddAuthentication(SimpleAuthDefaults.AuthenticationScheme)
                .AddCookie(SimpleAuthDefaults.AuthenticationScheme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseAuthorization();
            
            app.UseRouting();
            
            app.UseSimpleAuth();
            
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}