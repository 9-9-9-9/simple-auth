using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AddRoleCommand : AbstractCommand
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;
        private readonly IRoleManagementService _roleManagementService;

        public AddRoleCommand(IRoleManagementService roleManagementService,
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _roleManagementService = roleManagementService;
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        protected override Task DoMainJob(string[] args)
        {
            var subModules = new List<string>();
            var counter = 0;
            do
            {
                $"Sub module {++counter} (leave empty to submit)".Write();
                var input = Console.ReadLine()?.Trim();
                if (input.IsBlank())
                    break;
                subModules.Add(input);
            } while (true);
            
            return _roleManagementService.AddRoleAsync(new CreateRoleModel
            {
                Corp = _simpleAuthConfigurationProvider.Corp,
                App = _simpleAuthConfigurationProvider.App,
                Env = args[0],
                Tenant = args[1],
                Module = args[2],
                SubModules = subModules.ToArray()
            }).ContinueWith(_ => Print("Added"));
        }

        public override string[] GetParametersName()
        {
            return new[] {"Env", "Tenant", "Module"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0, 1, 2};
    }
}