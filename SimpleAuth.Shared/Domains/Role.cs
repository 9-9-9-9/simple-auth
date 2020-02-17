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
        public Verb Verb { get; set; }

        public ClientPermissionModel ToClientRoleModel()
        {
            RoleUtils.Parse(RoleId, out var clientRoleModel);
            clientRoleModel.Verb = Verb;
            return clientRoleModel;
        }

        public PermissionModel Cast()
        {
            return new PermissionModel
            {
                Role = RoleId,
                Verb = Verb.Serialize()
            };
        }
    }
    
    public static class RoleExtensions
    {
        public static IEnumerable<Role> DistinctRoles(this IEnumerable<Role> source)
        {
            foreach (var sameRoles in source.OrEmpty().GroupBy(r => r.RoleId))
            {
                var permission = Verb.None.Grant(sameRoles.Select(r => r.Verb).ToArray());
                
                if (permission == Verb.None)
                    continue;

                yield return new Role
                {
                    RoleId = sameRoles.Key,
                    Verb = permission
                };
            }
        }
    }
}