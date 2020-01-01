using System;
using System.ComponentModel.DataAnnotations;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public partial class RoleRecord : BaseEntity<Guid>, IPermissionRelated
    {
        [Index, Required] public string RoleId { get; set; }
        public Permission Permission { get; set; }
    }

    public partial class RoleRecord
    {
        public Shared.Domains.Role ToDomainObject()
        {
            return new Shared.Domains.Role
            {
                RoleId = RoleId,
                Permission = Permission
            };
        }
    }
}