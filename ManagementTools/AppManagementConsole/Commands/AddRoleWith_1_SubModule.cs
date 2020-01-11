using SimpleAuth.Client.Services;

namespace AppManagementConsole.Commands
{
    public class AddRoleWith_1_SubModule : AddRoleCommand
    {
        public AddRoleWith_1_SubModule(IRoleManagementService roleManagementService, ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider) : base(roleManagementService, simpleAuthConfigurationProvider)
        {
        }

        protected override byte NumberOfSubModules => 1;
    }
}