using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Client.Models
{
    public class SimpleAuthorizationClaim
    {
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
        public ClientRoleModel ClientRoleModel { get; set; }
        // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global

        // ReSharper disable UnusedMember.Global
        public SimpleAuthorizationClaim()
        {
        }
        // ReSharper restore UnusedMember.Global

        public SimpleAuthorizationClaim(ClientRoleModel clientRoleModel)
        {
            ClientRoleModel = clientRoleModel;
        }

        public SimpleAuthorizationClaim(RoleModel roleModel)
        {
            RoleUtils.Parse(roleModel.Role, roleModel.Permission, out var clientRoleModel);
            ClientRoleModel = clientRoleModel;
        }

        public bool Contains(SimpleAuthorizationClaim another)
        {
            var big = this;
            var small = another;

            return RoleUtils.ContainsOrEquals(big.ClientRoleModel, small.ClientRoleModel,
                RoleUtils.ComparisionFlag.All);
        }
    }

    public struct PackageSimpleAuthorizationClaim
    {
        [Required] public string UserId { get; set; }

        public SimpleAuthorizationClaim[] Claims { get; set; }

        public ICollection<SimpleAuthorizationClaim> ClaimsOrEmpty =>
            Claims.IsAny()
                ? Claims
                : Enumerable.Empty<SimpleAuthorizationClaim>().ToArray();
    }

    public static class SimpleAuthorizationClaimExtensions
    {
        public static ICollection<SimpleAuthorizationClaim> ToSimpleAuthorizationClaims(
            this ICollection<RoleModel> roleModels)
        {
            return roleModels.Select(x => new SimpleAuthorizationClaim(x)).ToList();
        }
    }
}