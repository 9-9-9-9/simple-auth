using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Extensions.PermissionGenerator.Attributes;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Extensions
{
    public class PermissionGenerator<TSaModuleAttribute, TSaPermissionAttribute> : IDisposable
        where TSaModuleAttribute : Attribute
        where TSaPermissionAttribute : Attribute
    {
        private const string ReservedKeywordGroupAdmin = "administrator";
        private const byte NumberOfSubModulesForAdminGroup = 10;
        
        private readonly Type _typeSaModuleAttr = typeof(TSaModuleAttribute);
        private readonly Type _typeSaPermissionAttr = typeof(TSaPermissionAttribute);

        private readonly PropertyInfo _propModule;
        private readonly PropertyInfo _propSubModules;
        private readonly PropertyInfo _propVerb;

        public PermissionGenerator(string corp, string app, params string[] environments)
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

            _corp = corp;
            _app = app;
            ForEnvironments(environments);
            
            if (_corp.IsBlank())
                throw new ArgumentNullException(nameof(corp));
            
            if (_app.IsBlank())
                throw new ArgumentNullException(nameof(app));
        }

        private readonly ICollection<Assembly> _targetAssemblies = new HashSet<Assembly>();
        private readonly string _corp;
        private readonly string _app;
        private readonly ICollection<string> _targetEnvs = new HashSet<string>();

        public PermissionGenerator<TSaModuleAttribute, TSaPermissionAttribute> ForEnvironments(params string[] envs)
        {
            envs.ToList().ForEach(_targetEnvs.Add);
            if (envs.Any(x => x.IsBlank()))
                throw new ArgumentException(nameof(envs));
            return this;
        }

        public PermissionGenerator<TSaModuleAttribute, TSaPermissionAttribute> AddAssembly(Assembly assembly)
        {
            _targetAssemblies.Add(assembly);
            return this;
        }

        public PermissionGenerator<TSaModuleAttribute, TSaPermissionAttribute> AddAssembly(Type type)
        {
            return AddAssembly(type.Assembly);
        }

        public PermissionGenerator<TSaModuleAttribute, TSaPermissionAttribute> AddAssembly<T>()
        {
            return AddAssembly(typeof(T));
        }

        public ICollection<(string, List<GeneratedPermissionItem>)> Scan()
        {
            return ConvertToPermissionItemsByGroup(GroupingByGroupName(ScanJob()))
                .Select(x => (x.Key, x.ToList()))
                .ToList();
        }

        public void ScanToFile(string fileName = null)
        {
            if (fileName != null && fileName.IsBlank())
                throw new ArgumentException(nameof(fileName));
            
            var outputFileName = fileName ?? $"PermissionGenerator-{DateTime.Now:yyyyMMdd:HHmmss}.tml";
            var records = Scan();

            var sb = new StringBuilder();
            sb.AppendLine("Users");
            sb.AppendLine();
            sb.AppendLine("Groups");
            sb.AppendLine($"\t{ReservedKeywordGroupAdmin}");
            sb.AppendLine($"\t\t{GenerateAdminRoleId(0)}\t{nameof(Verb.Full)}");
            for (var sm = 1; sm <= NumberOfSubModulesForAdminGroup; sm++)
                sb.AppendLine($"\t\t{GenerateAdminRoleId(sm)}\t{nameof(Verb.Full)}");

            foreach (var record in records)
            {
                sb.AppendLine();
                sb.AppendLine($"\t{record.Item1}");
                foreach (var generatedPermissionItem in record.Item2)
                {
                    var roleId = new ClientPermissionModel
                    {
                        Corp = _corp,
                        App = _app,
                        Env = generatedPermissionItem.Env,
                        Tenant = generatedPermissionItem.Tenant,
                        Module = generatedPermissionItem.Module,
                        SubModules = generatedPermissionItem.SubModules,
                    }.ComputeId();
                    sb.AppendLine($"\t\t{roleId}\t{generatedPermissionItem.Verb}");
                }
            }
            
            File.WriteAllText(outputFileName, sb.ToString());

            IEnumerable<string> YieldSubModules(int count)
            {
                while (count-- > 0)
                    yield return Constants.WildCard;
            }

            string GenerateAdminRoleId(int numberOfSubModules) => new ClientPermissionModel
            {
                Corp = _corp,
                App = _app,
                Env = Constants.WildCard,
                Tenant = Constants.WildCard,
                Module = Constants.WildCard,
                SubModules = YieldSubModules(numberOfSubModules).OrEmpty().ToArray()
            }.ComputeId();
        }

        private IEnumerable<IGrouping<string, GeneratedPermissionItem>> ConvertToPermissionItemsByGroup(
            IEnumerable<(string, IEnumerable<PermissionInfo>)> groupingByGroupName)
        {
            var generatedPermissionItems = new List<GeneratedPermissionItem>();

            foreach (var (groupName, permissionInfos) in groupingByGroupName)
            foreach (var permissionInfo in permissionInfos)
            {
                if (permissionInfo.Tenants.IsEmpty())
                    permissionInfo.Tenants = new[] {Constants.WildCard};

                foreach (var targetEnv in _targetEnvs)
                {
                    foreach (var tenant in permissionInfo.Tenants)
                    {
                        generatedPermissionItems.Add(new GeneratedPermissionItem
                        {
                            PermissionGroupName = groupName,
                            Env = targetEnv,
                            Tenant = tenant,
                            Module = permissionInfo.Module,
                            SubModules = permissionInfo.SubModules.OrEmpty().ToArray(),
                            Verb = permissionInfo.Verb
                        });
                    }
                }
            }

            return generatedPermissionItems.GroupBy(x => x.PermissionGroupName);
        }

        private IEnumerable<(string, IEnumerable<PermissionInfo>)> GroupingByGroupName(
            IEnumerable<PermissionInfo> permissionInfos)
        {
            var permissionInfoList = permissionInfos.OrEmpty().ToList();
            var groupNames = permissionInfoList.SelectMany(x => x.Groups).ToList();
            foreach (var groupName in groupNames)
                yield return (groupName, permissionInfoList.Where(x => x.Groups.Contains(groupName)));
        }

        private IEnumerable<PermissionInfo> ScanJob()
        {
            if (_targetAssemblies.IsEmpty())
                throw new InvalidOperationException(
                    $"No assembly provided, method '{nameof(AddAssembly)}' should be called"
                );

            if (_targetEnvs.IsEmpty())
                throw new InvalidOperationException(
                    $"No environment provided, method '{nameof(ForEnvironments)}' should be called"
                );

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

                    var groups = GetGroups(type).OrEmpty().Concat(GetGroups(methodInfo).OrEmpty()).ToArray();
                    var tenants = GetTenants(type).OrEmpty().Concat(GetTenants(methodInfo).OrEmpty()).ToArray();
                    
                    if (groups.Contains(ReservedKeywordGroupAdmin))
                        throw new ArgumentException($"Value of {nameof(SaGroupAttribute)} can not be reserved keyword '{ReservedKeywordGroupAdmin}'. This group will be auto-generated with full permissions up to {NumberOfSubModulesForAdminGroup} sub modules");

                    foreach (var permission in permissions)
                        yield return new PermissionInfo
                        {
                            Groups = groups,
                            Tenants = tenants,
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

            IEnumerable<string> GetGroups(MemberInfo memberInfo) =>
                memberInfo.GetCustomAttributes<SaGroupAttribute>().OrEmpty().Select(x => x.Name);

            IEnumerable<string> GetTenants(MemberInfo memberInfo) => memberInfo.GetCustomAttributes<SaTenantAttribute>()
                .OrEmpty().Select(x => x.Tenant);
        }

        public void Dispose()
        {
            _targetAssemblies.Clear();
            _targetEnvs.Clear();
        }
    }

    public class PermissionInfo
    {
        public string[] Groups { get; set; }
        public string[] Tenants { get; set; }
        public string Module { get; set; }
        public string[] SubModules { get; set; }
        public Verb Verb { get; set; }
    }

    public class GeneratedPermissionItem
    {
        public string PermissionGroupName { get; set; }
        public string Env { get; set; }
        public string Tenant { get; set; }
        public string Module { get; set; }
        public string[] SubModules { get; set; }
        public Verb Verb { get; set; }
    }
}