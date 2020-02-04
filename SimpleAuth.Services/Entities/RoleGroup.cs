using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Models;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public partial class RoleGroup : BaseEntity<Guid>, ILockable, ICorpRelated, IAppRelated
    {
        [Index, Required] public string Name { get; set; }

        public ICollection<RoleRecord> RoleRecords { get; set; }

        [Index] public bool Locked { get; set; }

        [Index, Required] public string Corp { get; set; }

        [Index, Required] public string App { get; set; }

        public ICollection<RoleGroupUser> RoleGroupUsers { get; set; }
    }

    public partial class RoleGroup
    {
        public Shared.Domains.RoleGroup ToDomainObject()
        {
            return new Shared.Domains.RoleGroup
            {
                Name = Name,
                Corp = Corp,
                App = App,
                Locked = Locked,
                Roles = RoleRecords.OrEmpty().Select(x => x.ToDomainObject()).ToArray(),
            };
        }
    }
}