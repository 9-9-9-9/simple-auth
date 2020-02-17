using System;
using System.ComponentModel.DataAnnotations;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public partial class PermissionRecord : BaseEntity<Guid>, IEnvRelated, ITenantRelated, IPermissionRelated
    {
        [Index, Required] public string RoleId { get; set; }
        [Index, Required] public string Env { get; set; }
        [Index, Required] public string Tenant { get; set; }
        public Verb Verb { get; set; }
    }

    public partial class PermissionRecord
    {
        public Shared.Domains.Permission ToDomainObject()
        {
            return new Shared.Domains.Permission
            {
                RoleId = RoleId,
                Verb = Verb
            };
        }
    }

    public static class RoleRecordExtensions
    {
        public static PermissionRecord ToEntityObject(this Shared.Domains.Permission permission)
        {
            return new PermissionRecord
            {
                RoleId = permission.RoleId,
                Verb = permission.Verb
            };
        }
    }
}