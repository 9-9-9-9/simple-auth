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
        Task AddRoleGroupAsync(CreateRoleGroupModel createRoleGroupModel);
        Task<RoleGroup> GetRoleGroupAsync(string roleGroupName);
        Task AddRoleToGroupAsync(string roleGroupName, UpdateRolesModel updateRolesModel);
        Task DeleteRolesAsync(string roleGroupName, params RoleModel[] roleModels);
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

        public Task AddRoleGroupAsync(CreateRoleGroupModel createRoleGroupModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.AddRoleGroup)
                    .Method(Constants.HttpMethods.POST),
                createRoleGroupModel.JsonSerialize()
            );
        }

        public Task<RoleGroup> GetRoleGroupAsync(string roleGroupName)
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<RoleGroup>(
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.GetRoles(roleGroupName))
                    .Method(Constants.HttpMethods.GET)
            );
        }

        public Task AddRoleToGroupAsync(string roleGroupName, UpdateRolesModel updateRolesModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.AddRoleToGroup(roleGroupName))
                    .Method(Constants.HttpMethods.POST),
                updateRolesModel.JsonSerialize()
            );
        }

        public Task DeleteRolesAsync(string roleGroupName, params RoleModel[] roleModels)
        {
            var nameValueCollection = new NameValueCollection();
            roleModels.ToList().ForEach(x => nameValueCollection["roles"] = RoleUtils.Merge(x.Role, x.Permission));
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