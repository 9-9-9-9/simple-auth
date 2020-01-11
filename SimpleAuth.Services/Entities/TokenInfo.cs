using System;
using System.ComponentModel.DataAnnotations;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public class TokenInfo : BaseEntity<Guid>, ICorpRelated, IAppRelated, IRowVersionedRecord
    {
        [Index, Required] public string Corp { get; set; }
        [Index] public string App { get; set; }
        public int Version { get; set; }
        public byte[] RowVersion { get; set; }
    }
}