using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Models;

namespace AppManagementConsole.Commands
{
    public class AddRoleCommand : AbstractCommand, ICommand
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
            return _roleManagementService.AddRoleAsync(new CreateRoleModel
            {
                Corp = _simpleAuthConfigurationProvider.Corp,
                App = _simpleAuthConfigurationProvider.App,
                Env = args[0],
                Tenant = args[1],
                Module = args[2],
                SubModules = args.Skip(3).Take(NumberOfSubModules).ToArray()
            });
        }

        protected virtual byte NumberOfSubModules => 0;

        public override string[] GetParametersName()
        {
            return YieldParametersName().ToArray();
            
            IEnumerable<string> YieldParametersName()
            {
                yield return "Env";
                yield return "Tenant";
                yield return "Module";
                for (var sm = 1; sm <= NumberOfSubModules; sm++)
                    yield return $"Sub Module {sm}";
            }
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => GetParametersName().Select((_, i) => i).ToArray();
    }
}