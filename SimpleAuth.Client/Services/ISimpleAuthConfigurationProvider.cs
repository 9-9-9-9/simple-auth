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
        string Corp { get; set; }
        string App { get; set; }
        string Env { get; }
        string Tenant { get; }
        bool LiveChecking { get; }
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

        private static string _corp;
        
        public string Corp
        {
            get => _corp = _corp.Or(_simpleAuthSettings.Corp);
            set => _corp = value;
        }

        private static string _app;

        public string App
        {
            get => _app = _app.Or(_simpleAuthSettings.App);
            set => _app = value;
        }

        public string Env => _simpleAuthSettings.Env;
        public string Tenant => _simpleAuthSettings.Tenant;
        public bool LiveChecking => _simpleAuthSettings.LiveChecking;
        public string EndPointUrl { get; }

        public Dictionary<string, string> OthersAppsTokens
        {
            get
            {
                var res = new Dictionary<string, string>();
                var tokens = _simpleAuthSettings?.TokenSettings?.OtherAppsTokens ?? new string[0];
                ParseTokensToDict(ref res, nameof(SimpleAuthTokenSettings.OtherAppsTokens), tokens);

                var secretTokens = _simpleAuthSettings?.TokenSettings?.OtherAppsSecretTokens ?? string.Empty;
                if (!secretTokens.IsBlank())
                {
                    tokens = secretTokens.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Any())
                        ParseTokensToDict(ref res,
                            $"Secret [{nameof(SimpleAuthSettings)}:{nameof(SimpleAuthSettings.TokenSettings)}:{nameof(SimpleAuthTokenSettings.OtherAppsSecretTokens)}]",
                            tokens);
                }

                return res;
            }
        }

        private void ParseTokensToDict(ref Dictionary<string, string> dict, string configSectionName,
            params string[] tokens)
        {
            var res = new Dictionary<string, string>();
            foreach (var token in tokens)
            {
                if (token.IsBlank())
                    continue;
                var spl = token.Trim().Split(new[] {' '}, 2, StringSplitOptions.None);
                if (spl.Length != 2 || spl.Any(x => x.IsBlank()))
                    throw new ArgumentException();
                if (res.ContainsKey(spl[0]))
                    throw new ArgumentException(
                        $"{configSectionName} has duplicated key '{spl[0]}'");
                res.Add(spl[0], spl[1]);
            }

            foreach (var kvp in res)
            {
                if (dict.ContainsKey(kvp.Key))
                    dict[kvp.Key] = kvp.Value;
                else
                    dict.Add(kvp.Key, kvp.Value);
            }
        }
    }
}