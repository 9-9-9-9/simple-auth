using System;
using System.Threading.Tasks;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    public interface IRoleGroupManagementService
    {
        Task AddRoleGroupAsync(CreateRoleGroupModel createRoleGroupModel);
        Task<Shared.Domains.RoleGroup> GetRoleGroupAsync(string roleGroupName);
        Task AddRoleToGroupAsync(string roleGroupName, UpdateRolesModel updateRolesModel);
        Task DeleteRolesAsync(string roleGroupName, DeleteRolesModel deleteRolesModel);
        Task DeleteAllRolesAsync(string roleGroupName);
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

        public Task<Shared.Domains.RoleGroup> GetRoleGroupAsync(string roleGroupName)
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<Shared.Domains.RoleGroup>(
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
                    .Method(Constants.HttpMethods.PUT),
                updateRolesModel.JsonSerialize()
            );
        }

        public Task DeleteRolesAsync(string roleGroupName, DeleteRolesModel deleteRolesModel)
        {
            if (deleteRolesModel.Roles.IsEmpty())
                throw new ArgumentNullException(nameof(deleteRolesModel.Roles));
            return SubmitDeleteRolesAsync(roleGroupName, deleteRolesModel);
        }

        public Task DeleteAllRolesAsync(string roleGroupName)
        {
            return SubmitDeleteRolesAsync(roleGroupName, new DeleteRolesModel
            {
                Roles = new RoleModel[0]
            });
        }

        private Task SubmitDeleteRolesAsync(string roleGroupName, DeleteRolesModel deleteRolesModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.DeleteRoles(roleGroupName))
                    .Method(Constants.HttpMethods.DELETE),
                deleteRolesModel.JsonSerialize()
            );
        }
    }
}