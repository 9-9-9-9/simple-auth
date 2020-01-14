using System;
using System.Collections.Generic;
using System.Linq;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.AspNetCore.Models
{
    public class ClaimsBuilder
    {
        public string Module { get; set; }
        public bool Restricted { get; set; } = true;
        public readonly HashSet<PermissionBatch> PermissionBatches = new HashSet<PermissionBatch>();

        public ClaimsBuilder WithModule(string module, bool restricted = true)
        {
            if (module.IsBlank())
                throw new ArgumentNullException(nameof(module));
            Module = module;
            Restricted = restricted;
            return this;
        }

        public ClaimsBuilder WithModule(SaModuleAttribute module)
        {
            return WithModule(module.Module, module.Restricted);
        }

        public ClaimsBuilder WithPermission(Permission permission, params string[] subModules)
        {
            if (subModules?.Any(x => x.IsBlank()) == true)
                throw new ArgumentException($"{nameof(subModules)} contains blank element");

            PermissionBatches.Add(new PermissionBatch
            {
                Permission = permission,
                SubModules = subModules ?? new string[0]
            });

            return this;
        }

        public ClaimsBuilder WithPermission(SaPermissionAttribute permission)
        {
            return WithPermission(permission.Permission, permission.SubModules);
        }

        public ClaimsBuilder WithPermissions(IEnumerable<SaPermissionAttribute> permissions)
        {
            foreach (var permission in permissions)
                WithPermission(permission.Permission, permission.SubModules);
            return this;
        }

        public IEnumerable<SimpleAuthorizationClaim> Build(string corp, string app, string env, string tenant)
        {
            foreach (var permissionBatch in PermissionBatches)
            {
                yield return new SimpleAuthorizationClaim(
                    new ClientRoleModel
                    {
                        Corp = corp,
                        App = app,
                        Env = env,
                        Tenant = tenant,
                        Module = Module,
                        SubModules = permissionBatch.SubModules,
                        Permission = permissionBatch.Permission
                    }
                );
            }
        }

        public IEnumerable<SimpleAuthorizationClaim> Build(
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            return Build(simpleAuthConfigurationProvider, simpleAuthConfigurationProvider.Tenant);
        }

        public IEnumerable<SimpleAuthorizationClaim> Build(
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider, string tenant)
        {
            return Build(simpleAuthConfigurationProvider.Corp, simpleAuthConfigurationProvider.App,
                simpleAuthConfigurationProvider.Env, tenant);
        }
    }

    public class PermissionBatch
    {
        public Permission Permission { get; set; }
        public string[] SubModules { get; set; } = new string[0];

        protected bool Equals(PermissionBatch other)
        {
            if (Permission != other.Permission)
                return false;
            if (SubModules.Length != other.SubModules.Length)
                return false;
            return SubModules.SequenceEqual(other.SubModules);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PermissionBatch) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                var subModules = SubModules;
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return ((int) Permission * 397) ^ (subModules != null ? subModules.GetHashCode() : 0);
            }
        }
    }
}