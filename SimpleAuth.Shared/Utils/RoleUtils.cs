using System;
using SimpleAuth.Core.Extensions;

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

        public static void Parse(string roleId, out string corp, out string app, out string env, out string tenant,
            out string module, out string[] subModules)
        {
            var spl = roleId.Split(new[] {Constants.ChSplitterRoleParts}, StringSplitOptions.None);
            if (spl.Length < 5 || spl.Length > 6)
                throw new ArgumentException(nameof(roleId));
            corp = spl[0];
            app = spl[1];
            env = spl[2];
            tenant = spl[3];
            module = spl[4];

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
        }
    }
}