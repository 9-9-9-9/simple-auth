using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Utils
{
    public static class RoleUtils
    {
        public static string ComputeRoleId(string corp, string app, string env, string tenant, string module,
            string joinedSubModules)
        {
            ThrowIfBlank(corp, app, env, tenant, module);

            var sb = new StringBuilder();
            sb.Append(corp);
            sb.Append(Constants.SplitterRoleParts);
            sb.Append(app);
            sb.Append(Constants.SplitterRoleParts);
            sb.Append(env);
            sb.Append(Constants.SplitterRoleParts);
            sb.Append(tenant);
            sb.Append(Constants.SplitterRoleParts);
            sb.Append(module);

            if (!joinedSubModules.IsBlank())
            {
                sb.Append(Constants.SplitterRoleParts);
                sb.Append(joinedSubModules);
            }

            return sb.ToString().NormalizeInput();
        }

        public static string ComputeRoleId(string corp, string app, string env, string tenant, string module,
            string[] subModules)
        {
            return ComputeRoleId(corp, app, env, tenant, module, JoinSubModules(subModules));
        }

        public static string JoinSubModules(ICollection<string> subModules)
        {
            subModules = subModules.OrEmpty().ToList();
            
            if (subModules.Count < 2)
                return subModules.FirstOrDefault().IsBlank() ? null : subModules.First();
            
            if (subModules.Any(x => x.IsBlank()))
                throw new ArgumentNullException($"{nameof(subModules)} contains blank string, which is not allowed");

            return string.Join(Constants.SplitterSubModules, subModules.Or(new string[0]));
        }

        private static void ThrowIfBlank(string corp, string app, string env, string tenant, string module)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));
            if (env.IsBlank())
                throw new ArgumentNullException(nameof(env));
            if (tenant.IsBlank())
                throw new ArgumentNullException(nameof(tenant));
            if (module.IsBlank())
                throw new ArgumentNullException(nameof(module));
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
                if (subModules.Length == 0 || (subModules.Length == 1 && subModules.First().IsBlank()))
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

        public static void Parse(string roleId, string permission, out ClientRoleModel clientRoleModel)
        {
            Parse(roleId, out var tmpClientRoleModel);
            tmpClientRoleModel.Permission = permission.Deserialize();
            clientRoleModel = tmpClientRoleModel;
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
                if (sBig == sSmall)
                    return true;
                if (sBig == Constants.WildCard)
                    return true;
                return false;
            }
        }

        public static IEnumerable<ClientRoleModel> Distinct(IEnumerable<ClientRoleModel> source)
        {
            var org = source.ToList();
            var toBeRemoved = new HashSet<ClientRoleModel>();
            foreach (var candidateBig in org)
            {
                if (toBeRemoved.Contains(candidateBig))
                    continue;
                foreach (var candidateSmall in org)
                {
                    // ReSharper disable once PossibleUnintendedReferenceComparison
                    if (candidateBig == candidateSmall)
                        continue;

                    if (toBeRemoved.Contains(candidateSmall))
                        continue;

                    if (ContainsOrEquals(candidateBig, candidateSmall, ComparisionFlag.All))
                        toBeRemoved.Add(candidateSmall);
                }
            }

            return org.Except(toBeRemoved);
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

        public static string Merge(string roleId, string permission)
        {
            if (roleId.IsBlank())
                throw new ArgumentNullException(nameof(roleId));

            if (permission.IsBlank())
                throw new ArgumentNullException(nameof(permission));

            if (permission == "0")
                throw new ArgumentException(nameof(permission));

            return $"{roleId}{Constants.ChSplitterMergedRoleIdWithPermission}{permission}";
        }

        public static string Merge(string roleId, Permission permission)
        {
            return Merge(roleId, permission.Serialize());
        }

        public static (string, Permission) UnMerge(string merged)
        {
            if (merged.IsBlank())
                throw new ArgumentNullException(nameof(merged));

            var spl = merged.Split(Constants.ChSplitterMergedRoleIdWithPermission);
            if (spl.Length != 2 || spl.Any(x => x.IsBlank()))
                throw new ArgumentException(nameof(merged));

            return (spl[0], spl[1].Deserialize());
        }
    }
}