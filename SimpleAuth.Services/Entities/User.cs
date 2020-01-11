using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public class User : BaseEntity<string>, IRowVersionedRecord
    {
        [Index(IsUnique = true), Required] public string NormalizedId { get; set; }
        public ICollection<RoleGroupUser> RoleGroupUsers { get; set; }

        public ICollection<LocalUserInfo> UserInfos { get; set; }
        public byte[] RowVersion { get; set; }
    }
}