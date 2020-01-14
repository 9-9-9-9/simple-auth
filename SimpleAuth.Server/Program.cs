using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SimpleAuthServer
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFiles()
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Getting the motors running...");

                CreateHostBuilder(args).Build().Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
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
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseSerilog();
    }

    internal static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddJsonFiles(this IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.AddJsonFile("/configmaps/simple-auth/appsettings.json", optional: true, reloadOnChange: true);
            return builder;
        }
    }
}