using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;
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
            var sb = new StringBuilder();
            sb.Append(Corp);
            sb.Append(Constants.SplitterRoleParts);
            sb.Append(App);
            sb.Append(Constants.SplitterRoleParts);
            sb.Append(Env);
            sb.Append(Constants.SplitterRoleParts);
            sb.Append(Tenant);
            sb.Append(Constants.SplitterRoleParts);
            sb.Append(Module);
            if (!SubModules.IsBlank())
            {
                sb.Append(Constants.SplitterRoleParts);
                sb.Append(SubModules);
            }

            Id = sb.ToString().NormalizeInput();
            return this;
        }
    }

    public static class RoleExtensions
    {
        public static string JoinSubModules(this IEnumerable<string> subModules)
        {
            return string.Join(Constants.SplitterSubModules, subModules.Or(new string[0]));
        }
        
        public static RoleRecord ToEntityObject(this SimpleAuth.Shared.Domains.Role role)
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