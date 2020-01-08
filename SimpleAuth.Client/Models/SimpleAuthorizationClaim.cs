using System.Collections.Generic;
using Newtonsoft.Json;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Client.Models
{
    public class SimpleAuthorizationClaims
    {
        public List<SimpleAuthorizationClaim> Claims { get; set; }
    }

    public class SimpleAuthorizationClaim
    {
        public string Tenant { get; set; }
        public string Module { get; set; }
        public string Permission { get; set; }

        public SimpleAuthorizationClaim()
        {
            
        }
        
        public SimpleAuthorizationClaim(string rolePartsFromModule, string permission, string tenant)
        {
            Tenant = tenant;
            Module = rolePartsFromModule;
            Permission = permission;
        }

        public SimpleAuthorizationClaim(RoleModel roleModel) : this(
            RoleUtils.CutPartsBefore(RoleUtils.RolePart.Module, roleModel.Role),
            roleModel.Permission,
            RoleUtils.TakeSinglePart(RoleUtils.RolePart.Tenant, roleModel.Role)
        )
        {
        }

        [JsonIgnore] public Permission PermissionEnum => Permission.Deserialize();
    }
}