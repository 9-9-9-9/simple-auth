using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Extensions;

namespace ConsoleApps.Shared
{
    public abstract class AbstractAppRunnable
    {
        protected abstract bool AllowSwitchingDefaultApp { get; }
        
        public virtual async Task RunAsync()
        {
            var builder = new ConfigurationBuilder();
            builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("/configmaps/management-tools/appsettings.json", optional: false, reloadOnChange: true);
            builder.AddUserSecrets(GetType().Assembly, false);
            var configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();
            services.Configure<SimpleAuthSettings>(configuration.GetSection(nameof(SimpleAuthSettings)));
            services = RegisterServiceCollections(services);

            var serviceProvider = services.BuildServiceProvider();

            var simpleAuthConfigurationProvider = serviceProvider.GetService<ISimpleAuthConfigurationProvider>();
            if (AllowSwitchingDefaultApp)
            {
                var othersAppsTokens = simpleAuthConfigurationProvider.OthersAppsTokens;
                if (othersAppsTokens.Any())
                {
                    $"Default app is: {simpleAuthConfigurationProvider.App}".Write();
                    "You can change to another app by typing another app name (leave empty to continue with default):"
                        .Write();
                    var switchToApp = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!switchToApp.IsBlank())
                    {
                        if (!othersAppsTokens.ContainsKey(switchToApp))
                        {
                            var validApps = othersAppsTokens.ToArray().Select(x => x.Key);
                            throw new ArgumentException(
                                $"Token of app '{switchToApp}' could not be found, please correct/insert into appsettings.json, key {nameof(SimpleAuthSettings.TokenSettings)}.{nameof(SimpleAuthTokenSettings.OtherAppsTokens)}. Valid values are '{string.Join(",", validApps)}'");
                        }

                        simpleAuthConfigurationProvider.App = switchToApp;
                        simpleAuthConfigurationProvider.AppToken = othersAppsTokens[switchToApp];
                    }
                }
            }

            var commandDict = serviceProvider
                .GetServices<ICommand>()
                .Select((x, i) => (i + 1, x))
                .ToDictionary(tp => tp.Item1, tp => tp.x);

            while (true)
            {
                "====================================".Write();
                "Commands".Write();
                commandDict.ToList().ForEach(kvp => $"{kvp.Key}. {kvp.Value.GetType().Name}".Write());
                "0. Exit".Write();
                if (AllowSwitchingDefaultApp)
                    $"Working application is '{simpleAuthConfigurationProvider.App}'".Write();
                "Select your option".Write();

                var opt = ReadInputInt();

                if (opt == 0)
                    Environment.Exit(0);

                if (!commandDict.ContainsKey(opt))
                    throw new InvalidOperationException($"There is no option number {opt}");

                var selectedCommand = commandDict[opt];
                var arguments = new List<string>();

                selectedCommand.GetParametersName().ToList().ForEach(x =>
                {
                    $"{x}: ".Write();
                    arguments.Add(GetInputString());
                });

                await ProcessCommand(selectedCommand, arguments.ToArray(), configuration, serviceProvider);
                "Execution of command was finished successfully".Write();
                Console.ReadLine();
            }
        }

        protected virtual Task ProcessCommand(ICommand command, string[] args, IConfigurationRoot configurationRoot,
            IServiceProvider serviceProvider)
        {
            return command.Process(args);
        }

        protected int ReadInputInt() => int.Parse(GetInputString());
        protected string GetInputString() => Console.ReadLine();


        protected abstract IServiceCollection RegisterServiceCollections(IServiceCollection serviceCollection);
    }
}