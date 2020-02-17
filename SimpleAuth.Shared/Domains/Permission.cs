using System.Collections.Generic;
using System.Linq;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Shared.Domains
{
    public class Permission : BaseDomain, IPermissionRelated
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
    
    public static class PermissionExtensions
    {
        public static IEnumerable<Permission> DistinctRoles(this IEnumerable<Permission> source)
        {
            foreach (var sameRoles in source.OrEmpty().GroupBy(r => r.RoleId))
            {
                var verb = Verb.None.Grant(sameRoles.Select(r => r.Verb).ToArray());
                
                if (verb == Verb.None)
                    continue;

                yield return new Permission
                {
                    RoleId = sameRoles.Key,
                    Verb = verb
                };
            }
        }
    }
}