using System;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Utils
{
    public static class RoleUtils
    {
        public static string JoinPartsFromModule(string module, string[] subModules)
        {
            return subModules.IsEmpty()
                ? module
                : $"{module}{Constants.SplitterRoleParts}{string.Join(Constants.SplitterSubModules, subModules)}";
        }

        public static void Parse(string roleId, out ClientRoleModel clientRoleModel)
        {
            var spl = roleId.Split(new[] {Constants.ChSplitterRoleParts}, StringSplitOptions.None);
            if (spl.Length < 5 || spl.Length > 6)
                throw new ArgumentException(nameof(roleId));

            string[] subModules;
            if (spl.Length == 6)
            {
                subModules = spl[5].Split(new[] {Constants.ChSplitterSubModules}, StringSplitOptions.None);
                if (subModules.Length == 0)
                    throw new ArgumentException($"{nameof(roleId)}: empty but declared submodules");
            }
            else
            {
                subModules = new string[0];
            }
            
            clientRoleModel = new ClientRoleModel
            {
                Corp = spl[0],
                App = spl[1],
                Env = spl[2],
                Tenant = spl[3],
                Module = spl[4],
                SubModules = subModules
            };
        }
    }
}