using System;
using System.Security.Claims;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Shared.Models
{
    public class ModuleClaim : Claim
    {
        public Permission Permission { get; }

        public ModuleClaim(string rolePartsFromModule, string permission, string issuer)
            : base(rolePartsFromModule, permission, nameof(Byte), issuer)
        {
            if (!byte.TryParse(permission, out var bp))
                throw new ArgumentException($"{nameof(permission)} is not byte");
            Permission = (Permission) bp;
        }

        public ModuleClaim(RoleModel roleModel, string issuer)
            : this(CutPartsBeforeModule(roleModel.Role), roleModel.Permission, issuer)
        {
        }

        internal static string CutPartsBeforeModule(string roleId)
        {
            return RoleUtils.CutPartsBefore(RoleUtils.RolePart.Module, roleId);
        }
    }
}