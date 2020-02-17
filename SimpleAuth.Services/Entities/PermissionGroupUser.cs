using System;
using System.ComponentModel.DataAnnotations;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public class PermissionGroupUser : BaseEntity
    {
        [Index, Required] public string UserId { get; set; }
        public User User { get; set; }

        [Index, Required] public Guid PermissionGroupId { get; set; }
        public PermissionGroup PermissionGroup { get; set; }
    }
}