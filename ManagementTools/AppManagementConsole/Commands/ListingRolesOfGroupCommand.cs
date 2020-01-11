using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;

namespace AppManagementConsole.Commands
{
    public class ListingRolesOfGroupCommand : AbstractCommand
    {
        private readonly IRoleGroupManagementService _roleGroupManagementService;

        public ListingRolesOfGroupCommand(IRoleGroupManagementService roleGroupManagementService)
        {
            _roleGroupManagementService = roleGroupManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            return Print(_roleGroupManagementService.GetRoleGroupAsync(args[0])
                .ContinueWith(x =>
                {
                    var roleGroup = x.Result;
                    var sb = new StringBuilder();

                    if (roleGroup.Roles.IsAny())
                        roleGroup.Roles.ToList().ForEach(rm => { sb.AppendLine($"<{rm.Permission,20}> {rm.RoleId}"); });
                    else
                        sb.AppendLine("Group doesn't have any role");
                    if (roleGroup.Locked)
                        sb.AppendLine($"Group {roleGroup.Name} is being LOCKED");

                    return sb.ToString();
                })
            );
        }

        public override string[] GetParametersName()
        {
            return new[] {"Group name"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0};
    }
}