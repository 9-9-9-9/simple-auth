using System.Threading.Tasks;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    public interface IUserManagementService
    {
        Task AssignUserToGroupsAsync(string userId, ModifyUserPermissionGroupsModel modifyUserPermissionGroupsModel);
        Task UnAssignUserFromAllGroupsAsync(string userId);
        Task CreateUserAsync(CreateUserModel createUserModel);
    }

    public class DefaultUserManagementService : ClientService, IUserManagementService
    {
        private readonly IHttpService _httpService;

        public DefaultUserManagementService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IHttpService httpService) : base(simpleAuthConfigurationProvider)
        {
            _httpService = httpService;
        }

        protected override RequestBuilder NewRequest()
        {
            return base.NewRequest()
                .WithAppToken();
        }

        public Task AssignUserToGroupsAsync(string userId, ModifyUserPermissionGroupsModel modifyUserPermissionGroupsModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.User.AssignUserToPermissionGroups(userId))
                    .Method(Constants.HttpMethods.POST),
                modifyUserPermissionGroupsModel.JsonSerialize()
            );
        }

        public Task UnAssignUserFromAllGroupsAsync(string userId)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.User.UnAssignUserFromAllGroupsAsync(userId))
                    .Method(Constants.HttpMethods.DELETE)
            );
        }

        public Task CreateUserAsync(CreateUserModel createUserModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.User.CreateUser)
                    .Method(Constants.HttpMethods.POST),
                createUserModel.JsonSerialize()
            );
        }
    }
}