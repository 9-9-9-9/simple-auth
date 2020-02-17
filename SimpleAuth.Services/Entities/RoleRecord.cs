using System;
using System.ComponentModel.DataAnnotations;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public partial class RoleRecord : BaseEntity<Guid>, IEnvRelated, ITenantRelated, IPermissionRelated
    {
        [Index, Required] public string RoleId { get; set; }
        [Index, Required] public string Env { get; set; }
        [Index, Required] public string Tenant { get; set; }
        public Verb Verb { get; set; }
    }

    public partial class RoleRecord
    {
        public Shared.Domains.Role ToDomainObject()
        {
            return new Shared.Domains.Role
            {
                RoleId = RoleId,
                Verb = Verb
            };
        }
    }

    public static class RoleRecordExtensions
    {
        public static RoleRecord ToEntityObject(this Shared.Domains.Role role)
        {
            return new RoleRecord
            {
                RoleId = role.RoleId,
                Verb = role.Verb
            };
        }
    }
}