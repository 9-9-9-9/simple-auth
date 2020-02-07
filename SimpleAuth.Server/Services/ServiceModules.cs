using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.DependencyInjection;
#pragma warning disable 1591

namespace SimpleAuth.Server.Services
{
    public class ServiceModules : RegistrableModules
    {
        public override void RegisterModules(IServiceCollection services)
        {
            services.AddSingleton<IGoogleService, DefaultGoogleService>();
            services.AddTransient<IHttpService, DefaultHttpService>();
        }
    }
}