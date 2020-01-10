using SimpleAuth.Client.Models;

namespace SimpleAuth.Client.Services
{
    public interface ISimpleAuthConfigurationProvider : IClientService
    {
        string MasterToken { get; }
        string CorpToken { get; }
        string AppToken { get; }
        string Corp { get; }
        string App { get; }
        string Env { get; }
        string Tenant { get; }
        string EndPointUrl { get; }
    }

    public class DefaultSimpleAuthConfigurationProvider : ISimpleAuthConfigurationProvider
    {
        private readonly SimpleAuthSettings _simpleAuthSettings;

        public DefaultSimpleAuthConfigurationProvider(SimpleAuthSettings simpleAuthSettings)
        {
            _simpleAuthSettings = simpleAuthSettings;
            EndPointUrl = _simpleAuthSettings.SimpleAuthServerUrl?.TrimEnd('/');
        }

        public string MasterToken => _simpleAuthSettings.TokenSettings.MasterToken;
        public string CorpToken => _simpleAuthSettings.TokenSettings.CorpToken;
        public string AppToken => _simpleAuthSettings.TokenSettings.AppToken;
        public string Corp => _simpleAuthSettings.Corp;
        public string App => _simpleAuthSettings.App;
        public string Env => _simpleAuthSettings.Env;
        public string Tenant => _simpleAuthSettings.Tenant;
        public string EndPointUrl { get; }
    }
}