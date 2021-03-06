using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public class User : BaseEntity<string>
    {
        [Index(IsUnique = true), Required] public string NormalizedId { get; set; }
        public ICollection<PermissionGroupUser> PermissionGroupUsers { get; set; }

        public ICollection<LocalUserInfo> UserInfos { get; set; }
    }
}