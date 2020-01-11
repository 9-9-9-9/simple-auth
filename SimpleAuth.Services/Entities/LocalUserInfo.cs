using System;
using System.ComponentModel.DataAnnotations;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public partial class LocalUserInfo : BaseEntity<Guid>, ICorpRelated, ILockable, IRowVersionedRecord
    {
        [Index, Required] public string UserId { get; set; }
        [Index] public string Email { get; set; }
        [Index] public string NormalizedEmail { get; set; }
        [Index, Required] public string Corp { get; set; }
        public string EncryptedPassword { get; set; }

        public bool Locked { get; set; }
        // TODO: implement Lock Out
        
        public byte[] RowVersion { get; set; }
    }

    public partial class LocalUserInfo
    {
        public Shared.Domains.LocalUserInfo ToDomainObject()
        {
            return new Shared.Domains.LocalUserInfo
            {
                Email = Email,
                Corp = Corp,
                Locked = Locked,
            };
        }
    }
}