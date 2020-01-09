using System;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Enums;
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

        public static void Parse(string roleId, string permission, out ClientRoleModel clientRoleModel)
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
                SubModules = subModules,
                Permission = permission.Deserialize()
            };
        }
        
        public static bool ContainsOrEquals(ClientRoleModel big, ClientRoleModel small, ComparisionFlag comparisionFlag)
        {
            if (comparisionFlag.HasFlag(ComparisionFlag.Corp) && !ContainsOrEquals(big.Corp, small.Corp))
                return false;
            if (comparisionFlag.HasFlag(ComparisionFlag.App) && !ContainsOrEquals(big.App, small.App))
                return false;
            if (comparisionFlag.HasFlag(ComparisionFlag.Env) && !ContainsOrEquals(big.Env, small.Env))
                return false;
            if (comparisionFlag.HasFlag(ComparisionFlag.Tenant) && !ContainsOrEquals(big.Tenant, small.Tenant))
                return false;
            if (comparisionFlag.HasFlag(ComparisionFlag.Module) && !ContainsOrEquals(big.Module, small.Module))
                return false;

            if (comparisionFlag.HasFlag(ComparisionFlag.SubModule))
            {
                var bigNoOfSubModules = big.SubModules?.Length ?? 0;
                var smallNoOfSubModules = small.SubModules?.Length ?? 0;
                if (bigNoOfSubModules != smallNoOfSubModules)
                    return false;
                if (bigNoOfSubModules > 0)
                {
                    // ReSharper disable PossibleNullReferenceException
                    for (var i = 0; i < big.SubModules.Length; i++)
                    {
                        var smBig = big.SubModules[i];
                        var smSmall = small.SubModules[i];

                        if (!ContainsOrEquals(smBig, smSmall))
                            return false;
                    }
                    // ReSharper restore PossibleNullReferenceException
                }
            }

            if (comparisionFlag.HasFlag(ComparisionFlag.Permission))
                return big.Permission.HasFlag(small.Permission);
            else
                return true;

            bool ContainsOrEquals(string sBig, string sSmall)
            {
                if (sBig.IsBlank())
                    throw new ArgumentNullException(nameof(sBig));
                if (sSmall.IsBlank())
                    throw new ArgumentNullException(nameof(sSmall));
                if (sSmall == Constants.WildCard)
                    throw new ArgumentException($"Argument '{nameof(sSmall)}' can not be a wildcard");
                if (sBig == sSmall)
                    return true;
                if (sBig == Constants.WildCard)
                    return true;
                return false;
            }
        }

        [Flags]
        public enum ComparisionFlag : byte
        {
            Corp = 1,
            App = 2,
            Env = 4,
            Tenant = 8,
            Module = 16,
            SubModule = 32,
            Permission = 64,
            FromSubModules = SubModule | Permission,
            FromModule = Module | FromSubModules,
            FromTenant = Tenant | FromModule,
            FromEnv = Env | FromTenant,
            FromApp = App | FromEnv,
            All = Corp | FromApp
        }
    }
}