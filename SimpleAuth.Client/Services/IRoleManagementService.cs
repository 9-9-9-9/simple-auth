using System.Threading.Tasks;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    public interface IRoleManagementService
    {
        Task AddRoleAsync(CreateRoleModel createRoleModel);
    }

    public class DefaultRoleManagementService : ClientService, IRoleManagementService
    {
        private readonly IHttpService _httpService;

        public DefaultRoleManagementService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IHttpService httpService) : base(simpleAuthConfigurationProvider)
        {
            _httpService = httpService;
        }

        protected override RequestBuilder NewRequest()
        {
            return base.NewRequest()
                .WithAppToken();
        }

        public async Task AddRoleAsync(CreateRoleModel createRoleModel)
        {
            await _httpService.DoHttpRequestWithoutResponseAsync(
                true,
                NewRequest()
                    .Append(EndpointBuilder.Role.AddRole)
                    .Method(Constants.HttpMethods.POST),
                createRoleModel.JsonSerialize()
            );
        }
    }
}