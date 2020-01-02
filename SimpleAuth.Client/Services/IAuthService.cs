using System.Threading.Tasks;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    public interface IAuthService : IClientService
    {
        Task<ResponseUserModel> GetRolesAsync(string userId);
        Task<ResponseUserModel> LoginUsingPasswordAsync(string userId, string password);
        Task<ResponseUserModel> LoginUsingGoogleTokenAsync(LoginByGoogleRequest loginByGoogleRequest);
    }

    public class DefaultAuthService : ClientService, IAuthService
    {
        private readonly IHttpService _httpService;

        public DefaultAuthService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IHttpService httpService) : base(simpleAuthConfigurationProvider)
        {
            _httpService = httpService;
        }

        protected override RequestBuilder NewRequest()
        {
            return base.NewRequest().WithAppToken();
        }

        public async Task<ResponseUserModel> GetRolesAsync(string userId)
        {
            return await _httpService.DoHttpRequest2Async<ResponseUserModel>(
                NewRequest()
                .Append(EndpointConstants.User.GetActiveRoles(userId))
                .Method(Constants.HttpMethods.GET)
                .WithAppToken()
            );
        }

        public async Task<ResponseUserModel> LoginUsingPasswordAsync(string userId, string password)
        {
            return await _httpService.DoHttpRequest2Async<ResponseUserModel>(
                NewRequest()
                .Append(EndpointConstants.User.CheckPass(userId))
                .Method(Constants.HttpMethods.POST)
                .WithAppToken(),
                password
            );
        }

        public async Task<ResponseUserModel> LoginUsingGoogleTokenAsync(LoginByGoogleRequest loginByGoogleRequest)
        {
            return await _httpService.DoHttpRequest2Async<ResponseUserModel>(
                NewRequest()
                    .Append(EndpointConstants.User.CheckGoogleToken(loginByGoogleRequest.Email))
                    .Method(Constants.HttpMethods.POST)
                    .WithAppToken(),
                loginByGoogleRequest.JsonSerialize()
            );
        }
    }
}