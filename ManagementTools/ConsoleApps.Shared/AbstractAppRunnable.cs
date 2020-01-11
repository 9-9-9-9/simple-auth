using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Core.Extensions;
using System.Linq;

namespace ConsoleApps.Shared
{
    public abstract class AbstractAppRunnable
    {
        public virtual async Task RunAsync()
        {
            IServiceCollection services = new ServiceCollection();
            services = RegisterServiceCollections(services);

            var serviceProvider = services.BuildServiceProvider();

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

                await ProcessCommand(selectedCommand, arguments.ToArray());
                Console.ReadLine();
            }
        }

        protected virtual Task ProcessCommand(ICommand command, string[] args)
        {
            return command.Process(args);
        }

        protected int ReadInputInt() => int.Parse(GetInputString());
        protected string GetInputString() => Console.ReadLine();
        

        protected abstract IServiceCollection RegisterServiceCollections(IServiceCollection serviceCollection);
    }
}