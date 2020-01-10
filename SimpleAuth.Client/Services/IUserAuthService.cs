using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using SimpleAuth.Client.Exceptions;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IUserAuthService : IClientService
    {
        Task<ResponseUserModel> GetUserAsync(string userId);
        Task<ResponseUserModel> GetUserAsync(string userId, string password);
        Task<ResponseUserModel> GetUserAsync(LoginByGoogleRequest loginByGoogleRequest);
        Task<bool> DoesUserHavePermissionAsync(string userId, string roleId, Permission permission);
    }

    public class DefaultUserAuthService : ClientService, IUserAuthService
    {
        private readonly IHttpService _httpService;

        public DefaultUserAuthService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IHttpService httpService) : base(simpleAuthConfigurationProvider)
        {
            _httpService = httpService;
        }

        protected override RequestBuilder NewRequest()
        {
            return base.NewRequest()
                .WithAppToken()
                .WithFilterEnv()
                .WithFilterTenant();
        }

        public async Task<ResponseUserModel> GetUserAsync(string userId)
        {
            return await _httpService.DoHttpRequest2Async<ResponseUserModel>(
                NewRequest()
                .Append(EndpointBuilder.User.GetActiveRoles(userId))
                .Method(Constants.HttpMethods.GET)
            );
        }

        public async Task<ResponseUserModel> GetUserAsync(string userId, string password)
        {
            return await _httpService.DoHttpRequest2Async<ResponseUserModel>(
                NewRequest()
                .Append(EndpointBuilder.User.CheckPass(userId))
                .Method(Constants.HttpMethods.POST),
                password
            );
        }

        public async Task<ResponseUserModel> GetUserAsync(LoginByGoogleRequest loginByGoogleRequest)
        {
            return await _httpService.DoHttpRequest2Async<ResponseUserModel>(
                NewRequest()
                    .Append(EndpointBuilder.User.CheckGoogleToken(loginByGoogleRequest.Email))
                    .Method(Constants.HttpMethods.POST),
                loginByGoogleRequest.JsonSerialize()
            );
        }

        public async Task<bool> DoesUserHavePermissionAsync(string userId, string roleId, Permission permission)
        {
            var res = await _httpService.DoHttpRequestAsync<string>(
                NewRequest()
                    .Append(EndpointBuilder.User.CheckUserPermission(userId, roleId, permission.Serialize()))
                    .Method(Constants.HttpMethods.GET)
            );
            if (res.Item2 == HttpStatusCode.OK)
                return true;
            if (res.Item2 == HttpStatusCode.NotAcceptable)
                return false;
            throw new SimpleAuthHttpRequestException(res.Item2);
        }
    }
}