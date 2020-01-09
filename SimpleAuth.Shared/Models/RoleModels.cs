using System.ComponentModel.DataAnnotations;
using System.Text;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;

namespace SimpleAuth.Shared.Models
{
    public class CreateRoleModel : 
        ICorpRelated, IAppRelated, 
        IEnvRelated, ITenantRelated, 
        IModuleRelated 
    {
        [Required]
        public string Corp { get; set; }
        
        [Required]
        public string App { get; set; }
        
        [Required]
        public string Env { get; set; }
        
        [Required]
        public string Tenant { get; set; }
        
        [Required]
        public string Module { get; set; }
        
        public string[] SubModules { get; set; }
    }

    public class RoleModel
    {
        public string Role { get; set; }
        public string Permission { get; set; }

        public static RoleModel Cast(Role role)
        {
            return new RoleModel
            {
                Role = role.RoleId,
                Permission = role.Permission.Serialize()
            };
        }
    }

    public class UpdateRolesModel
    {
        [Required]
        public RoleModel[] Roles { get; set; }
    }

    public class DeleteRolesModel
    {
        public RoleModel[] Roles { get; set; }
    }

    public class ClientRoleModel : ICorpRelated, IAppRelated, IEnvRelated, ITenantRelated, IModuleRelated, IRawSubModulesRelated
    {
        public string Corp { get; set; }
        public string App { get; set; }
        public string Env { get; set; }
        public string Tenant { get; set; }
        public string Module { get; set; }
        public string[] SubModules { get; set; }
        public Permission Permission { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder($"{Corp}.{App}.{Env}.{Tenant}.{Module}");
            if (SubModules.IsAny())
            {
                sb.Append('.');
                sb.Append(string.Join(Constants.SplitterSubModules, SubModules));
            }

            sb.Append($", Permission: {Permission}");
            return sb.ToString();
        }
    }
}