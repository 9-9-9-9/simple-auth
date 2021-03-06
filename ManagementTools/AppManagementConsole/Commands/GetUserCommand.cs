using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApps.Shared.Commands;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Enums;

namespace AppManagementConsole.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
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
                var model = x.Result;

                if (model == null)
                    return "No user content";
                
                var sb = new StringBuilder();
                

                sb.AppendLine($"Email: {model.Email}");
                sb.AppendLine($"Token expire: {model.ExpiryDate?.ToString() ?? "None"} secs");
                sb.AppendLine($"Active roles:");
                model.ActiveRoles?.ToList().ForEach(rm => {sb.AppendLine($"<{rm.Verb.Deserialize(),20}> {rm.Role}");});
                
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