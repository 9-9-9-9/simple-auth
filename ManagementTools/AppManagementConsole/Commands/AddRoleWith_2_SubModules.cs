using SimpleAuth.Client.Services;

namespace AppManagementConsole.Commands
{
    public class AddRoleWith_2_SubModules : AddRoleCommand
    {
        public AddRoleWith_2_SubModules(IRoleManagementService roleManagementService, ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider) : base(roleManagementService, simpleAuthConfigurationProvider)
        {
        }

        protected override byte NumberOfSubModules => 2;
    }
}