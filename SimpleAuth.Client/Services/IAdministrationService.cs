using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleAuth.Client.Utils;
using SimpleAuth.Shared;

namespace SimpleAuth.Client.Services
{
    public interface IAdministrationService : IClientService
    {
        Task<string> GenerateCorpPermissionTokenAsync(string corp);
        Task<string> GenerateAppPermissionTokenAsync(string corp, string app);
        Task<string> GenerateAppPermissionTokenAsync(string app);
        Task<string> EncryptUsingMasterEncryptionKey(string data);
        Task<string> DecryptUsingMasterEncryptionKey(string data);
    }

    public class DefaultAdministrationService : ClientService, IAdministrationService
    {
        private readonly IHttpService _httpService;

        public DefaultAdministrationService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            IHttpService httpService) : base(simpleAuthConfigurationProvider)
        {
            _httpService = httpService;
        }

        protected override RequestBuilder NewRequest()
        {
            return base.NewRequest()
                .WithMasterToken();
        }

        private RequestBuilder NewRequestForCorpManagement()
        {
            return base.NewRequest()
                .WithCorpToken();
        }

        public async Task<string> GenerateCorpPermissionTokenAsync(string corp)
        {
            return await _httpService.DoHttpRequestWithResponseContentAsync<string>(
                NewRequest()
                    .Append(EndpointBuilder.Administration.GenerateCorpPermissionToken(corp))
                    .Method(Constants.HttpMethods.GET)
            );
        }

        public async Task<string> GenerateAppPermissionTokenAsync(string corp, string app)
        {
            return await _httpService.DoHttpRequestWithResponseContentAsync<string>(
                NewRequest()
                    .Append(EndpointBuilder.Administration.GenerateAppPermissionToken(corp, app))
                    .Method(Constants.HttpMethods.GET)
            );
        }

        public async Task<string> GenerateAppPermissionTokenAsync(string app)
        {
            return await _httpService.DoHttpRequestWithResponseContentAsync<string>(
                NewRequestForCorpManagement()
                    .Append(EndpointBuilder.Administration.GenerateAppPermissionToken(app))
                    .Method(Constants.HttpMethods.GET)
            );
        }

        public async Task<string> EncryptUsingMasterEncryptionKey(string data)
        {
            return await _httpService.DoHttpRequestWithResponseContentAsync<string>(
                NewRequest()
                    .Append(EndpointBuilder.Administration.EncryptPlainText())
                    .WithQuery(new Dictionary<string, string>
                    {
                        {"data", data}
                    })
                    .Method(Constants.HttpMethods.GET)
            );
        }

        public async Task<string> DecryptUsingMasterEncryptionKey(string data)
        {
            return await _httpService.DoHttpRequestWithResponseContentAsync<string>(
                NewRequest()
                    .Append(EndpointBuilder.Administration.DecryptData())
                    .WithQuery(new Dictionary<string, string>
                    {
                        {"data", data}
                    })
                    .Method(Constants.HttpMethods.GET)
            );
        }
    }
}