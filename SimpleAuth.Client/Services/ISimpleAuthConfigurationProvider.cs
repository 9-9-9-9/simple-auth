using SimpleAuth.Client.Models;

namespace SimpleAuth.Client.Services
{
    public interface ISimpleAuthConfigurationProvider : IClientService
    {
        string CorpToken { get; }
        string AppToken { get; }
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

        public string CorpToken => _simpleAuthSettings.CorpToken;
        public string AppToken => _simpleAuthSettings.AppToken;
        public string Env => _simpleAuthSettings.Env;
        public string Tenant => _simpleAuthSettings.Tenant;
        public string EndPointUrl { get; }
    }
}