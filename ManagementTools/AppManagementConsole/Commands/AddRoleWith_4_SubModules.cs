using SimpleAuth.Client.Services;

namespace AppManagementConsole.Commands
{
    public class AddRoleWith_4_SubModules : AddRoleCommand
    {
        public AddRoleWith_4_SubModules(IRoleManagementService roleManagementService, ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider) : base(roleManagementService, simpleAuthConfigurationProvider)
        {
        }

        protected override byte NumberOfSubModules => 4;
    }
}