using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Services.Entities
{
    public partial class Role : BaseEntity<string>,
        ICorpRelated,
        IAppRelated,
        IEnvRelated,
        ITenantRelated,
        IModuleRelated,
        ISubModuleRelated,
        ILockable
    {
        [Index, Required] public string Corp { get; set; }
        [Index, Required] public string App { get; set; }
        [Required] public string Env { get; set; }
        [Required] public string Tenant { get; set; }
        [Required] public string Module { get; set; }
        public string SubModules { get; set; }
        public bool Locked { get; set; }
    }

    public partial class Role
    {
        public Role ComputeId()
        {
            Id = RoleUtils.ComputeRoleId(Corp, App, Env, Tenant, Module, SubModules);
            return this;
        }
    }

    public static class RoleExtensions
    {
        public static string JoinSubModules(this IEnumerable<string> subModules)
        {
            return RoleUtils.JoinSubModules(subModules);
        }
        
        public static RoleRecord ToEntityObject(this Shared.Domains.Role role)
        {
            return new RoleRecord
            {
                RoleId = role.RoleId,
                Permission = role.Permission
            };
        }
        
        public static Role ConvertToEntity(this CreateRoleModel model)
        {
            var entity = new Role()
            {
                Corp = model.Corp,
                App = model.App,
                Env = model.Env,
                Tenant = model.Tenant,
                Module = model.Module,
                SubModules = model.SubModules.JoinSubModules()
            };
            entity.ComputeId();
            return entity;
        }
    }
}