using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;

namespace AppManagementConsole.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ListingPermissionsOfGroupCommand : AbstractCommand
    {
        private readonly IPermissionGroupManagementService _permissionGroupManagementService;

        public ListingPermissionsOfGroupCommand(IPermissionGroupManagementService permissionGroupManagementService)
        {
            _permissionGroupManagementService = permissionGroupManagementService;
        }

        protected override Task DoMainJob(string[] args)
        {
            return Print(_permissionGroupManagementService.GetPermissionGroupAsync(args[0])
                .ContinueWith(x =>
                {
                    var permissionGroup = x.Result;
                    var sb = new StringBuilder();

                    if (permissionGroup.Permissions.IsAny())
                        permissionGroup.Permissions.ToList().ForEach(rm => { sb.AppendLine($"<{rm.Verb,20}> {rm.RoleId}"); });
                    else
                        sb.AppendLine("Group doesn't have any permission");
                    if (permissionGroup.Locked)
                        sb.AppendLine($"Group {permissionGroup.Name} is being LOCKED");

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