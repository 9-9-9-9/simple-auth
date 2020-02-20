using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public partial class PermissionGroup : BaseEntity<Guid>, ILockable, ICorpRelated, IAppRelated
    {
        [Index, Required] public string Name { get; set; }
        
        public string Description { get; set; }

        public ICollection<PermissionRecord> PermissionRecords { get; set; }

        [Index] public bool Locked { get; set; }

        [Index, Required] public string Corp { get; set; }

        [Index, Required] public string App { get; set; }

        public ICollection<PermissionGroupUser> PermissionGroupUsers { get; set; }
    }

    public partial class PermissionGroup
    {
        public Shared.Domains.PermissionGroup ToDomainObject()
        {
            return new Shared.Domains.PermissionGroup
            {
                Name = Name,
                Description = Description,
                Corp = Corp,
                App = App,
                Locked = Locked,
                Permissions = PermissionRecords.OrEmpty().Select(x => x.ToDomainObject()).ToArray(),
            };
        }
    }
}