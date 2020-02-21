using System.Collections.Generic;
using System.Linq;
using SimpleAuth.Shared.Extensions;
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

        public ClientPermissionModel ToClientPermissionModel()
        {
            RoleUtils.Parse(RoleId, out var clientPermissionModel);
            clientPermissionModel.Verb = Verb;
            return clientPermissionModel;
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