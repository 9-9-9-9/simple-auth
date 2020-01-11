using System.Threading.Tasks;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    public interface IUserManagementService
    {
        Task AssignUserToGroupsAsync(string userId, ModifyUserRoleGroupsModel modifyUserRoleGroupsModel);
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

        public Task AssignUserToGroupsAsync(string userId, ModifyUserRoleGroupsModel modifyUserRoleGroupsModel)
        {
            return _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.User.AssignUserToRoleGroups(userId))
                    .Method(Constants.HttpMethods.PUT),
                modifyUserRoleGroupsModel.JsonSerialize()
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