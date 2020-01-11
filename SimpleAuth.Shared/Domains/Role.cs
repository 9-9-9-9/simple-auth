using System.Collections.Generic;
using System.Linq;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Shared.Domains
{
    public class Role : BaseDomain, IPermissionRelated
    {
        public string RoleId { get; set; }
        public bool Locked { get; set; }
        public Permission Permission { get; set; }

        public ClientRoleModel ToClientRoleModel()
        {
            RoleUtils.Parse(RoleId, out var clientRoleModel);
            clientRoleModel.Permission = Permission;
            return clientRoleModel;
        }

        public RoleModel Cast()
        {
            return new RoleModel
            {
                Role = RoleId,
                Permission = Permission.Serialize()
            };
        }
    }
    
    public static class RoleExtensions
    {
        public static IEnumerable<Role> DistinctRoles(this IEnumerable<Role> source)
        {
            foreach (var sameRoles in source.OrEmpty().GroupBy(r => r.RoleId))
            {
                var permission = Permission.None.Grant(sameRoles.Select(r => r.Permission).ToArray());
                
                if (permission == Permission.None)
                    continue;

                yield return new Role
                {
                    RoleId = sameRoles.Key,
                    Permission = permission
                };
            }
        }
    }
}