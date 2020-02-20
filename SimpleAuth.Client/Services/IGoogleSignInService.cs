using System.Threading.Tasks;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    public interface IGoogleSignInService
    {
        Task<ResponseUserModel> GetUserAsync(LoginByGoogleRequest loginByGoogleRequest);
        Task<string> GetPublicSignInClientId();
    }

    public class DefaultGoogleSignInService : ClientService, IGoogleSignInService
    {
        private readonly IUserAuthService _userAuthService;
        private readonly IHttpService _httpService;

        public DefaultGoogleSignInService(IUserAuthService userAuthService, IHttpService httpService,
            ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider) : base(simpleAuthConfigurationProvider)
        {
            _userAuthService = userAuthService;
            _httpService = httpService;
        }

        public Task<ResponseUserModel> GetUserAsync(LoginByGoogleRequest loginByGoogleRequest)
        {
            return _userAuthService.GetUserAsync(loginByGoogleRequest);
        }

        public Task<string> GetPublicSignInClientId()
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<string>(
                base.NewRequest()
                    .WithAppToken()
                    .Append(EndpointBuilder.ExternalLoginProvider.GetPublicGoogleSignInClientId)
                    .Method(Constants.HttpMethods.GET)
            );
        }
    }
}