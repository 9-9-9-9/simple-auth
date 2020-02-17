using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using SimpleAuth.Client.Exceptions;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Client.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IUserAuthService : IClientService
    {
        Task<PermissionModel[]> GetActiveRolesAsync(string userId);
        Task<ResponseUserModel> GetUserAsync(string userId);
        Task<ResponseUserModel> GetUserAsync(string userId, string password);
        Task<ResponseUserModel> GetUserAsync(LoginByGoogleRequest loginByGoogleRequest);
        Task<bool> DoesUserHavePermissionAsync(string userId, string roleId, Verb verb);
        Task<PermissionModel[]> GetMissingRolesAsync(string userId, PermissionModels permissionModels);
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

        public Task<PermissionModel[]> GetActiveRolesAsync(string userId)
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<PermissionModel[]>(
                NewRequest()
                    .Append(EndpointBuilder.User.GetActiveRoles(userId))
                    .Method(Constants.HttpMethods.GET)
            );
        }

        public Task<ResponseUserModel> GetUserAsync(string userId)
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<ResponseUserModel>(
                NewRequest()
                .Append(EndpointBuilder.User.GetUser(userId))
                .Method(Constants.HttpMethods.GET)
            );
        }

        public Task<ResponseUserModel> GetUserAsync(string userId, string password)
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<ResponseUserModel>(
                NewRequest()
                .Append(EndpointBuilder.User.CheckPass(userId))
                .Method(Constants.HttpMethods.POST),
                password
            );
        }

        public Task<ResponseUserModel> GetUserAsync(LoginByGoogleRequest loginByGoogleRequest)
        {
            return _httpService.DoHttpRequestWithResponseContentAsync<ResponseUserModel>(
                NewRequest()
                    .Append(EndpointBuilder.User.CheckGoogleToken(loginByGoogleRequest.Email))
                    .Method(Constants.HttpMethods.POST),
                loginByGoogleRequest.JsonSerialize()
            );
        }

        public async Task<bool> DoesUserHavePermissionAsync(string userId, string roleId, Verb verb)
        {
            var res = await _httpService.DoHttpRequestAsync<string>(
                NewRequest()
                    .Append(EndpointBuilder.User.CheckUserPermission(userId, roleId, verb.Serialize()))
                    .Method(Constants.HttpMethods.GET)
            );
            if (res.Item2 == HttpStatusCode.OK)
                return true;
            if (res.Item2 == HttpStatusCode.NotAcceptable)
                return false;
            
            throw new SimpleAuthHttpRequestException(res.Item2);
        }

        public async Task<PermissionModel[]> GetMissingRolesAsync(string userId, PermissionModels permissionModels)
        {
            var res = await _httpService.DoHttpRequestAsync<PermissionModel[]>(
                NewRequest()
                    .Append(EndpointBuilder.User.GetMissingPermissions(userId))
                    .Method(Constants.HttpMethods.POST),
                permissionModels.JsonSerialize()
            );
            if (res.Item2 == HttpStatusCode.OK)
                return new PermissionModel[0];
            
            if (res.Item2 == HttpStatusCode.NotAcceptable)
            {
                if (!res.Item3.IsAny())
                    throw new DataVerificationMismatchException($"Expected array of {nameof(PermissionModel)} as response content");
                return res.Item3;
            }
            
            throw new SimpleAuthHttpRequestException(res.Item2);
        }
    }
}