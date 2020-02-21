using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SimpleAuth.Shared.Extensions;
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

    public class PermissionModel
    {
        public string Role { get; set; }
        public string Verb { get; set; }

        public static PermissionModel Cast(Permission permission)
        {
            return new PermissionModel
            {
                Role = permission.RoleId,
                Verb = permission.Verb.Serialize()
            };
        }
    }

    public class PermissionModels
    {
        [Required]
        public PermissionModel[] Permissions { get; set; }
    }

    public partial class ClientPermissionModel : ICorpRelated, IAppRelated, IEnvRelated, ITenantRelated, IModuleRelated, IRawSubModulesRelated
    {
        public string Corp { get; set; }
        public string App { get; set; }
        public string Env { get; set; }
        public string Tenant { get; set; }
        public string Module { get; set; }
        public string[] SubModules { get; set; }
        public Verb Verb { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder($"{Corp}.{App}.{Env}.{Tenant}.{Module}");
            if (SubModules.IsAny())
            {
                sb.Append('.');
                sb.Append(string.Join(Constants.SplitterSubModules, SubModules));
            }

            sb.Append($", Permission: {Verb}");
            return sb.ToString();
        }

        public Permission ToRole()
        {
            return new Permission
            {
                RoleId = RoleUtils.ComputeRoleId(Corp, App, Env, Tenant, Module, SubModules),
                Verb = Verb
            };
        }

        protected bool Equals(ClientPermissionModel other)
        {
            return Corp == other.Corp && App == other.App && Env == other.Env && Tenant == other.Tenant && Module == other.Module && SubModules.SequenceEqual(other.SubModules) && Verb == other.Verb;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClientPermissionModel) obj);
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
                hashCode = (hashCode * 397) ^ (int) Verb;
                // ReSharper restore NonReadonlyMemberInGetHashCode
                return hashCode;
            }
        }
    }
    
    public partial class ClientPermissionModel
    {
        public string ComputeId()
        {
            return RoleUtils.ComputeRoleId(Corp, App, Env, Tenant, Module, SubModules);
        }

        public static ClientPermissionModel From(string roleId, string verb)
        {
            RoleUtils.Parse(roleId, verb, out var clientRoleModel);
            return clientRoleModel;
        }

        public static ClientPermissionModel From(string roleId, Verb verb)
        {
            RoleUtils.Parse(roleId, out var clientPermissionModel);
            clientPermissionModel.Verb = verb;
            return clientPermissionModel;
        }
    }

    public static class ClientRoleModelExtensions
    {
        public static IEnumerable<ClientPermissionModel> DistinctPermissions(this IEnumerable<ClientPermissionModel> clientPermissionModels)
        {
            return RoleUtils.Distinct(clientPermissionModels);
        }
    }
}