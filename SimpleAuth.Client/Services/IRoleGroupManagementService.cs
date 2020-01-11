using System.Threading.Tasks;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    public interface IRoleGroupManagementService
    {
        Task AddRoleGroupAsync(CreateRoleGroupModel createRoleGroupModel);
        Task<Shared.Domains.RoleGroup> GetRoleGroupAsync(string roleGroupName);
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

        public async Task AddRoleGroupAsync(CreateRoleGroupModel createRoleGroupModel)
        {
            await _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.AddRoleGroup)
                    .Method(Constants.HttpMethods.POST),
                createRoleGroupModel.JsonSerialize()
            );
        }

        public async Task<Shared.Domains.RoleGroup> GetRoleGroupAsync(string roleGroupName)
        {
            return await _httpService.DoHttpRequestWithResponseContentAsync<Shared.Domains.RoleGroup>(
                NewRequest()
                    .Append(EndpointBuilder.RoleGroupManagement.GetRoles(roleGroupName))
                    .Method(Constants.HttpMethods.GET)
            );
        }
    }
}