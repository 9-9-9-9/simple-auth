using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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

        public static RoleModel Cast(Domains.Role role)
        {
            return new RoleModel
            {
                Role = role.RoleId,
                Permission = role.Permission.Serialize()
            };
        }

        public ModuleClaim ToClaim()
        {
            return new ModuleClaim(this, Constants.Identity.Issuer);
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
}