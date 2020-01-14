using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Client.AspNetCore.Services;
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

        public ClaimsBuilder()
        {
        }

        public ClaimsBuilder(string module, Permission permission, params string[] subModules)
        {
            WithModule(module);
            WithPermission(permission, subModules);
        }

        public ClaimsBuilder(string module, params (Permission, string[])[] permissions)
        {
            WithModule(module);
            foreach (var permission in permissions)
                WithPermission(permission.Item1, permission.Item2);
        }

        public ClaimsBuilder WithModule(string module, bool restricted = true)
        {
            if (module.IsBlank())
                throw new ArgumentNullException(nameof(module));
            if (!Module.IsBlank())
                throw new ArgumentException(
                    $"Overriding property {nameof(Module)} is not allowed when already has value");

            Module = module;
            Restricted = restricted;
            return this;
        }

        public ClaimsBuilder WithModule(SaModuleAttribute module)
        {
            return WithModule(module.Module, module.Restricted);
        }

        public ClaimsBuilder WithModule(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var saModuleAttribute = methodInfo.GetCustomAttribute<SaModuleAttribute>();
            if (saModuleAttribute != null)
                return WithModule(saModuleAttribute);

            var classType = methodInfo.DeclaringType;
            return WithModule(classType);
        }

        public ClaimsBuilder WithModule(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var saModuleAttribute = type.GetCustomAttribute<SaModuleAttribute>();
            if (saModuleAttribute != null)
                WithModule(saModuleAttribute);

            return this;
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

        public ClaimsBuilder WithPermissions(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var saPermissionAttributes = methodInfo.GetCustomAttributes<SaPermissionAttribute>();
            return WithPermissions(saPermissionAttributes);
        }

        public ClaimsBuilder LoadFromMeta(MethodInfo methodInfo)
        {
            return
                WithModule(methodInfo)
                    .WithPermissions(methodInfo);
        }

        public ClaimsBuilder ClearModule()
        {
            Module = null;
            return this;
        }

        public ClaimsBuilder ClearAllPermissions()
        {
            PermissionBatches.Clear();
            return this;
        }

        public IEnumerable<SimpleAuthorizationClaim> Build(string corp, string app, string env, string tenant)
        {
            if (corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            if (app.IsBlank())
                throw new ArgumentNullException(nameof(app));
            if (env.IsBlank())
                throw new ArgumentNullException(nameof(env));
            if (tenant.IsBlank())
                throw new ArgumentNullException(nameof(tenant));
            if (Module.IsBlank())
                throw new ArgumentNullException(
                    $"{nameof(Module)}, perhaps you missed calling method {nameof(WithModule)}");
            if (PermissionBatches.IsEmpty())
                throw new ArgumentNullException(
                    $"No permission provided, perhaps you missed calling method {nameof(WithPermission)} or {nameof(WithPermissions)}");

            if (PermissionBatches.Any(x => x.Permission == Permission.None))
                throw new ArgumentException($"Non-allowed permission value {nameof(Permission.None)} is existing");

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

        public IEnumerable<SimpleAuthorizationClaim> Build(HttpContext httpContext)
        {
            return Build(
                httpContext.RequestServices.GetService<ISimpleAuthConfigurationProvider>(),
                httpContext.RequestServices.GetService<ITenantProvider>().GetTenant(httpContext)
            );
        }

        public IEnumerable<SimpleAuthorizationClaim> Build(
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider, string tenant)
        {
            return Build(simpleAuthConfigurationProvider.Corp,
                simpleAuthConfigurationProvider.App,
                simpleAuthConfigurationProvider.Env,
                tenant
            );
        }

        public static ClaimsBuilder FromMetaData(MethodInfo methodInfo)
        {
            return new ClaimsBuilder()
                .LoadFromMeta(methodInfo);
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