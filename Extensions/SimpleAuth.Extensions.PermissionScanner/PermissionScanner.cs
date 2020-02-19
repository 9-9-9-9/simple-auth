using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Extensions
{
    public class PermissionScanner<TSaModuleAttribute, TSaPermissionAttribute> : IDisposable
        where TSaModuleAttribute : Attribute
        where TSaPermissionAttribute : Attribute
    {
        private readonly Type _typeSaModuleAttr = typeof(TSaModuleAttribute);
        private readonly Type _typeSaPermissionAttr = typeof(TSaPermissionAttribute);

        private readonly PropertyInfo _propModule;
        private readonly PropertyInfo _propSubModules;
        private readonly PropertyInfo _propVerb;

        public PermissionScanner()
        {
            if (!"SaModuleAttribute".Equals(_typeSaModuleAttr.Name))
                throw new ArgumentException(
                    $"Generic type {nameof(TSaModuleAttribute)} must be type of SaModuleAttribute");
            if (!"SaPermissionAttribute".Equals(_typeSaPermissionAttr.Name))
                throw new ArgumentException(
                    $"Generic type {nameof(TSaPermissionAttribute)} must be type of SaPermissionAttribute");

            _propModule = _typeSaModuleAttr.GetProperty("Module", BindingFlags.Instance | BindingFlags.Public);
            _propSubModules =
                _typeSaPermissionAttr.GetProperty("SubModules", BindingFlags.Instance | BindingFlags.Public);
            _propVerb = _typeSaPermissionAttr.GetProperty("Verb", BindingFlags.Instance | BindingFlags.Public);
        }

        private readonly ICollection<Assembly> _targetAssemblies = new List<Assembly>();

        public PermissionScanner<TSaModuleAttribute, TSaPermissionAttribute> AddAssembly(Assembly assembly)
        {
            _targetAssemblies.Add(assembly);
            return this;
        }

        public PermissionScanner<TSaModuleAttribute, TSaPermissionAttribute> AddAssembly(Type type)
        {
            return AddAssembly(type.Assembly);
        }

        public PermissionScanner<TSaModuleAttribute, TSaPermissionAttribute> AddAssembly<T>()
        {
            return AddAssembly(typeof(T));
        }

        public IEnumerable<PermissionInfo> Scan()
        {
            return RoleUtils.Distinct(ScanJob().Select(x => new ClientPermissionModel
            {
                Corp = "x",
                App = "x",
                Env = "x",
                Tenant = "x",
                Module = x.Module,
                SubModules = x.SubModules,
                Verb = x.Verb
            })).Select(x => new PermissionInfo
            {
                Module = x.Module,
                SubModules = x.SubModules,
                Verb = x.Verb
            });
        }
        
        private IEnumerable<PermissionInfo> ScanJob()
        {
            if (_targetAssemblies.IsEmpty())
                throw new InvalidOperationException(
                    $"No assembly provided, method '{nameof(AddAssembly)}' should be called");

            foreach (var type in _targetAssemblies.SelectMany(x => x.GetTypes()))
            {
                var moduleFromClass = GetModule(type);
                foreach (var methodInfo in type.GetMethods())
                {
                    var permissions = GetPermissions(methodInfo).ToList();
                    if (permissions.IsEmpty())
                        continue;

                    var module = GetModule(methodInfo) ?? moduleFromClass;
                    if (module == null)
                        throw new InvalidOperationException(
                            $"Wrong config of method {methodInfo.Name} of class {type.FullName} in assembly {type.Assembly.FullName}: {_typeSaPermissionAttr.Name} defined but missing {_typeSaModuleAttr.Name}"
                        );

                    foreach (var permission in permissions)
                        yield return new PermissionInfo
                        {
                            Module = module,
                            SubModules = permission.Item2 ?? new string[0],
                            Verb = permission.Item1
                        };
                }
            }

            string GetModule(MemberInfo memberInfo)
            {
                var attr = memberInfo.GetCustomAttribute<TSaModuleAttribute>();
                if (attr == null)
                    return null;
                return _propModule.GetValue(attr) as string;
            }

            IEnumerable<(Verb, string[])> GetPermissions(MemberInfo memberInfo)
            {
                var attrs = memberInfo.GetCustomAttributes<TSaPermissionAttribute>().OrEmpty().ToList();
                if (!attrs.Any())
                    yield break;
                foreach (var attr in attrs)
                {
                    var verb = (Verb) _propVerb.GetValue(attr);
                    var subModules = _propSubModules.GetValue(attr) as string[] ?? new string[0];
                    yield return (verb, subModules);
                }
            }
        }

        public void ScanToFile(string fileName = null)
        {
            var outputFileName = fileName ?? $"PermissionScanner-{DateTime.Now:yyyyMMdd:HHmmss}.txt";
            File.WriteAllText(outputFileName,
                "# The following permissions contains only modules, submodules and verbs\n");

            var sb = new StringBuilder();

            foreach (var permissionInfo in Scan())
            {
                sb.Append('\n');
                sb.Append(permissionInfo.Module);
                if (permissionInfo.SubModules.Any())
                {
                    sb.Append(Constants.SplitterRoleParts);
                    sb.Append(string.Join(Constants.SplitterSubModules, permissionInfo.SubModules));
                }

                foreach (var verb in RoleUtils.ParseToMinimum(permissionInfo.Verb))
                {
                    sb.Append('\t');
                    sb.Append(verb);
                }
            }

            File.WriteAllText(outputFileName, sb.ToString());
        }

        public void Dispose()
        {
            _targetAssemblies.Clear();
        }
    }

    public class PermissionInfo
    {
        public string Module { get; set; }
        public string[] SubModules { get; set; }
        public Verb Verb { get; set; }
    }
}