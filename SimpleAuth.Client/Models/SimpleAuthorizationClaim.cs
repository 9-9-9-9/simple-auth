using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Client.Models
{
    public class SimpleAuthorizationClaim
    {
        public string Tenant { get; set; }
        public string Module { get; set; }
        public string[] SubModules { get; set; }

        public Permission Permission { get; set; }

        public SimpleAuthorizationClaim()
        {
        }

        public SimpleAuthorizationClaim(string tenant, string module, string[] subModules, string permission)
            : this(tenant, module, subModules, permission.Deserialize())
        {
        }

        public SimpleAuthorizationClaim(string tenant, string module, string[] subModules, Permission permission)
        {
            Tenant = tenant;
            Module = module;
            SubModules = subModules.OrEmpty().ToArray();
            Permission = permission;
        }

        public SimpleAuthorizationClaim(RoleModel roleModel)
        {
            RoleUtils.Parse(roleModel.Role, out _, out _, out _, out var tenant, out var module, out var submodules);
            Tenant = tenant;
            Module = module;
            SubModules = submodules;
            Permission = roleModel.Permission.Deserialize();
        }

        public bool Contains(SimpleAuthorizationClaim another)
        {
            var big = this;
            var small = another;

            if (!ContainsOrEquals(big.Tenant, small.Tenant))
                return false;
            if (!ContainsOrEquals(big.Module, small.Module))
                return false;
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

            return big.Permission.HasFlag(small.Permission);
        }

        private bool ContainsOrEquals(string left, string right)
        {
            if (left.IsBlank())
                throw new ArgumentNullException(nameof(left));
            if (right.IsBlank())
                throw new ArgumentNullException(nameof(right));
            if (right == Constants.WildCard)
                throw new ArgumentException($"Argument '{nameof(right)}' can not be a wildcard");
            if (left == right)
                return true;
            if (left == Constants.WildCard)
                return true;
            return false;
        }
    }
    
    public static class SimpleAuthorizationClaimExtensions
    {
        public static IEnumerable<SimpleAuthorizationClaim> ToSimpleAuthorizationClaims(this IEnumerable<RoleModel> roleModels)
        {
            return roleModels.Select(x => new SimpleAuthorizationClaim(x));
        }

        public static Claim ToClaim(this IEnumerable<SimpleAuthorizationClaim> simpleAuthorizationClaims, Func<SimpleAuthorizationClaim[], string> serializer)
        {
            return new Claim(SimpleAuthDefaults.ClaimType, serializer(simpleAuthorizationClaims.OrEmpty().ToArray()));
        }

        public static Claim OfSimpleAuth(this IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(x => x.Type == SimpleAuthDefaults.ClaimType);
        }
    }
}