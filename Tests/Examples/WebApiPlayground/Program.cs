using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Extensions;

namespace WebApiPlayground
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var scanner = new PermissionGenerator<SaModuleAttribute, SaPermissionAttribute>(
                "test",
                "wap",
                "staging", "prod"
            );
            scanner
                .AddAssembly(typeof(Program).Assembly)
                .ScanToFile();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostBuilder, builder) =>
                    {
                        builder
                            .AddJsonFiles()
                            .AddEnvironmentVariables();
                    }
                )
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    internal static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddJsonFiles(this IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.AddJsonFile("/configmaps/test/wap/appsettings.json", optional: true, reloadOnChange: true);
            return builder;
        }
    }
}