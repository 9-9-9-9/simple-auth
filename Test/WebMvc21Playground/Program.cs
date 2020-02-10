using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WebMvc21Playground
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostBuilder, builder) =>
                    {
                        builder
                            .AddJsonFiles()
                            .AddEnvironmentVariables();
                    }
                )
                .UseStartup<Startup>();
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
