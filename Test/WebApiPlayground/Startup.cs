using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using WebApiPlayground.Services;

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
                TokenSettings = new SimpleAuthTokenSettings
                {
                    CorpToken = "",
                    AppToken = ""
                },
                Corp = "c",
                App = "a",
                Env = "e",
                Tenant = "t"
            });
            
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            
            // dummy
            services.AddSingleton<IAuthService, DummyAuthService>();
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