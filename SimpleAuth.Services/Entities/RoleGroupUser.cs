using System;
using System.ComponentModel.DataAnnotations;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public class RoleGroupUser : BaseEntity
    {
        [Index, Required] public string UserId { get; set; }
        public User User { get; set; }

        [Index, Required] public Guid RoleGroupId { get; set; }
        public RoleGroup RoleGroup { get; set; }
    }
}