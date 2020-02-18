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
    public interface IPermissionGroupManagementService
    {
        Task AddPermissionGroupAsync(CreatePermissionGroupModel createPermissionGroupModel);
        Task<PermissionGroup> GetPermissionGroupAsync(string permissionGroupName);
        Task AddPermissionToGroupAsync(string permissionGroupName, PermissionModels updatePermissionsModel);
        Task RevokePermissionsAsync(string permissionGroupName, params PermissionModel[] permissionModels);
        Task RevokeAllPermissionsAsync(string permissionGroupName);
        Task SetLockPermissionGroup(string permissionGroupName, bool @lock);
    }

    public class DefaultPermissionGroupManagementService : ClientService, IPermissionGroupManagementService
    {
        private readonly IHttpService _httpService;

        public DefaultPermissionGroupManagementService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IHttpService httpService) : base(simpleAuthConfigurationProvider)
        {
            _httpService = httpService;
        }

        protected override RequestBuilder NewRequest()
        {
            return base.NewRequest()
                .WithAppToken();
        }

        public Task AddPermissionGroupAsync(CreatePermissionGroupModel createPermissionGroupModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.PermissionGroupManagement.AddPermissionGroup)
                    .Method(Constants.HttpMethods.POST),
                createPermissionGroupModel.JsonSerialize()
            );
        }

        public Task<PermissionGroup> GetPermissionGroupAsync(string permissionGroupName)
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<PermissionGroup>(
                NewRequest()
                    .Append(EndpointBuilder.PermissionGroupManagement.GetPermissions(permissionGroupName))
                    .Method(Constants.HttpMethods.GET)
            );
        }

        public Task AddPermissionToGroupAsync(string permissionGroupName, PermissionModels updatePermissionsModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.PermissionGroupManagement.AddPermissionToGroup(permissionGroupName))
                    .Method(Constants.HttpMethods.POST),
                updatePermissionsModel.JsonSerialize()
            );
        }

        public Task RevokePermissionsAsync(string permissionGroupName, params PermissionModel[] permissionModels)
        {
            var nameValueCollection = new NameValueCollection();
            permissionModels.ToList().ForEach(x => nameValueCollection["roles"] = RoleUtils.Merge(x.Role, x.Verb));
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .WithQuery(nameValueCollection)
                    .WithoutContentType()
                    .Append(EndpointBuilder.PermissionGroupManagement.DeletePermissions(permissionGroupName))
                    .Method(Constants.HttpMethods.DELETE)
            );
        }

        public Task RevokeAllPermissionsAsync(string permissionGroupName)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .WithQuery("all", true.ToString())
                    .Append(EndpointBuilder.PermissionGroupManagement.DeletePermissions(permissionGroupName))
                    .Method(Constants.HttpMethods.DELETE)
            );
        }

        public Task SetLockPermissionGroup(string permissionGroupName, bool @lock)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .WithoutContentType()
                    .Append(EndpointBuilder.PermissionGroupManagement.UpdateLock(permissionGroupName))
                    .Method(@lock ? Constants.HttpMethods.POST : Constants.HttpMethods.DELETE)
            );
        }
    }
}