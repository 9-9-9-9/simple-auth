using SimpleAuth.Client.Services;

namespace AppManagementConsole.Commands
{
    public class AddRoleWith_3_SubModules : AddRoleCommand
    {
        public AddRoleWith_3_SubModules(IRoleManagementService roleManagementService, ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider) : base(roleManagementService, simpleAuthConfigurationProvider)
        {
        }

        protected override byte NumberOfSubModules => 3;
    }
}