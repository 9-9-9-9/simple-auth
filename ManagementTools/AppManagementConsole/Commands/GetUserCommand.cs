using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Enums;

namespace AppManagementConsole.Commands
{
    public class GetUserCommand : AbstractCommand
    {
        private readonly IUserAuthService _userAuthService;

        public GetUserCommand(IUserAuthService userAuthService)
        {
            _userAuthService = userAuthService;
        }

        protected override Task DoMainJob(string[] args)
        {
            var userId = args[0];
            return Print(_userAuthService.GetUserAsync(userId).ContinueWith(x =>
            {
                var sb = new StringBuilder();
                var model = x.Result;

                sb.AppendLine($"Email: {model.Email}");
                sb.AppendLine($"Token expire: {model.TokenExpireAfterSeconds?.ToString() ?? "None"} secs");
                sb.AppendLine($"Active roles:");
                model.ActiveRoles?.ToList().ForEach(rm => {sb.AppendLine($"<{rm.Permission.Deserialize(),20}> {rm.Role}");});
                
                return sb.ToString();
            }));
        }

        public override string[] GetParametersName()
        {
            return new[] {"User name"};
        }

        protected override IEnumerable<string> GetOthersArgumentsProblems(params string[] args)
        {
            yield break;
        }

        protected override int[] IdxParametersCanNotBeBlank => new[] {0};
    }
}