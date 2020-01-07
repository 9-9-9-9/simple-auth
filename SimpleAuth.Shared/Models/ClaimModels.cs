using System;
using System.Security.Claims;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Shared.Models
{
    public class ModuleClaim : Claim
    {
        public Permission Permission { get; }
        public string Tenant { get; set; }

        public ModuleClaim(string rolePartsFromModule, string permission, string tenant, string issuer)
            : base(
                rolePartsFromModule,
                permission,
                nameof(Byte),
                issuer
            )
        {
            if (!byte.TryParse(permission, out var bp))
                throw new ArgumentException($"{nameof(permission)} is not byte");
            Permission = (Permission) bp;
            Tenant = tenant;
        }

        public ModuleClaim(RoleModel roleModel, string issuer)
            : this(
                RoleUtils.CutPartsBefore(RoleUtils.RolePart.Module, roleModel.Role),
                roleModel.Permission,
                RoleUtils.TakeSinglePart(RoleUtils.RolePart.Tenant, roleModel.Role),
                issuer
            )
        {
        }
    }
}