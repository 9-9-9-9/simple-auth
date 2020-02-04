using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Utils;

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

    public class RoleModels
    {
        [Required]
        public RoleModel[] Roles { get; set; }
    }

    public partial class ClientRoleModel : ICorpRelated, IAppRelated, IEnvRelated, ITenantRelated, IModuleRelated, IRawSubModulesRelated
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

        public Role ToRole()
        {
            return new Role
            {
                RoleId = RoleUtils.ComputeRoleId(Corp, App, Env, Tenant, Module, SubModules),
                Permission = Permission
            };
        }

        protected bool Equals(ClientRoleModel other)
        {
            return Corp == other.Corp && App == other.App && Env == other.Env && Tenant == other.Tenant && Module == other.Module && SubModules.SequenceEqual(other.SubModules) && Permission == other.Permission;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClientRoleModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = (Corp != null ? Corp.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (App != null ? App.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Env != null ? Env.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tenant != null ? Tenant.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Module != null ? Module.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SubModules != null ? SubModules.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Permission;
                // ReSharper restore NonReadonlyMemberInGetHashCode
                return hashCode;
            }
        }
    }
    
    public partial class ClientRoleModel
    {
        public string ComputeId()
        {
            return RoleUtils.ComputeRoleId(Corp, App, Env, Tenant, Module, SubModules);
        }

        public static ClientRoleModel From(string roleId, string permission)
        {
            RoleUtils.Parse(roleId, permission, out var clientRoleModel);
            return clientRoleModel;
        }

        public static ClientRoleModel From(string roleId, Permission permission)
        {
            RoleUtils.Parse(roleId, out var clientRoleModel);
            clientRoleModel.Permission = permission;
            return clientRoleModel;
        }
    }

    public static class ClientRoleModelExtensions
    {
        public static IEnumerable<ClientRoleModel> DistinctRoles(this IEnumerable<ClientRoleModel> clientRoleModels)
        {
            return RoleUtils.Distinct(clientRoleModels);
        }
    }
}