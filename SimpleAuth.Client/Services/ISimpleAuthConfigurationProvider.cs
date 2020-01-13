using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using SimpleAuth.Client.Models;
using SimpleAuth.Core.Extensions;

namespace SimpleAuth.Client.Services
{
    public interface ISimpleAuthConfigurationProvider : IClientService
    {
        string MasterToken { get; }
        string CorpToken { get; }
        string AppToken { get; set; }
        string Corp { get; }
        string App { get; set; }
        string Env { get; }
        string Tenant { get; }
        string EndPointUrl { get; }
        Dictionary<string, string> OthersAppsTokens { get; }
    }

    public class DefaultSimpleAuthConfigurationProvider : ISimpleAuthConfigurationProvider
    {
        private readonly SimpleAuthSettings _simpleAuthSettings;

        public DefaultSimpleAuthConfigurationProvider(IOptions<SimpleAuthSettings> simpleAuthSettings)
        {
            _simpleAuthSettings = simpleAuthSettings.Value;
            EndPointUrl = _simpleAuthSettings.SimpleAuthServerUrl?.TrimEnd('/');
        }

        public string MasterToken => _simpleAuthSettings.TokenSettings.MasterToken;
        public string CorpToken => _simpleAuthSettings.TokenSettings.CorpToken;

        private static string _appToken;

        public string AppToken
        {
            get => _appToken = _appToken.Or(_simpleAuthSettings.TokenSettings.AppToken);
            set => _appToken = value;
        }
        public string Corp => _simpleAuthSettings.Corp;

        private static string _app;

        public string App
        {
            get => _app = _app.Or(_simpleAuthSettings.App);
            set => _app = value;
        }
        public string Env => _simpleAuthSettings.Env;
        public string Tenant => _simpleAuthSettings.Tenant;
        public string EndPointUrl { get; }

        public Dictionary<string, string> OthersAppsTokens
        {
            get
            {
                var res = new Dictionary<string, string>();
                var tokens = _simpleAuthSettings?.TokenSettings?.OtherAppsTokens ?? new string[0];
                foreach (var token in tokens)
                {
                    if (token.IsBlank())
                        continue;
                    var spl = token.Split(new[] {' '}, 2, StringSplitOptions.None);
                    if (spl.Length != 2 || spl.Any(x => x.IsBlank()))
                        throw new ArgumentException();
                    if (res.ContainsKey(spl[0]))
                        throw new ArgumentException(
                            $"{nameof(SimpleAuthTokenSettings.OtherAppsTokens)} has duplicated key '{spl[0]}'");
                    res.Add(spl[0], spl[1]);
                }

                return res;
            }
        }
    }
}