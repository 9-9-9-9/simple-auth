using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Domains;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Utils;

namespace SimpleAuth.Client.Services
{
    public interface IRoleGroupManagementService
    {
        Task AddRoleGroupAsync(CreatePermissionGroupModel createPermissionGroupModel);
        Task<PermissionGroup> GetRoleGroupAsync(string roleGroupName);
        Task AddRoleToGroupAsync(string roleGroupName, PermissionModels updatePermissionsModel);
        Task DeleteRolesAsync(string roleGroupName, params PermissionModel[] roleModels);
        Task DeleteAllRolesAsync(string roleGroupName);
        Task SetLockRoleGroup(string roleGroupName, bool @lock);
    }

    public class DefaultRoleGroupManagementService : ClientService, IRoleGroupManagementService
    {
        private readonly IHttpService _httpService;

        public DefaultRoleGroupManagementService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IHttpService httpService) : base(simpleAuthConfigurationProvider)
        {
            _httpService = httpService;
        }

        protected override RequestBuilder NewRequest()
        {
            return base.NewRequest()
                .WithAppToken();
        }

        public Task AddRoleGroupAsync(CreatePermissionGroupModel createPermissionGroupModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.AddRoleGroup)
                    .Method(Constants.HttpMethods.POST),
                createPermissionGroupModel.JsonSerialize()
            );
        }

        public Task<PermissionGroup> GetRoleGroupAsync(string roleGroupName)
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<PermissionGroup>(
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.GetRoles(roleGroupName))
                    .Method(Constants.HttpMethods.GET)
            );
        }

        public Task AddRoleToGroupAsync(string roleGroupName, PermissionModels updatePermissionsModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.AddRoleToGroup(roleGroupName))
                    .Method(Constants.HttpMethods.POST),
                updatePermissionsModel.JsonSerialize()
            );
        }

        public Task DeleteRolesAsync(string roleGroupName, params PermissionModel[] roleModels)
        {
            var nameValueCollection = new NameValueCollection();
            roleModels.ToList().ForEach(x => nameValueCollection["roles"] = RoleUtils.Merge(x.Role, x.Verb));
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .WithQuery(nameValueCollection)
                    .WithoutContentType()
                    .Append(EndpointBuilder.RoleGroupManagement.DeleteRoles(roleGroupName))
                    .Method(Constants.HttpMethods.DELETE)
            );
        }

        public Task DeleteAllRolesAsync(string roleGroupName)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .WithQuery("all", true.ToString())
                    .Append(EndpointBuilder.RoleGroupManagement.DeleteRoles(roleGroupName))
                    .Method(Constants.HttpMethods.DELETE)
            );
        }

        public Task SetLockRoleGroup(string roleGroupName, bool @lock)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .WithoutContentType()
                    .Append(EndpointBuilder.RoleGroupManagement.UpdateLock(roleGroupName))
                    .Method(@lock ? Constants.HttpMethods.POST : Constants.HttpMethods.DELETE)
            );
        }
    }
}