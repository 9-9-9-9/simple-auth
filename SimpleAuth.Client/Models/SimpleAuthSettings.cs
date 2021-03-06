namespace SimpleAuth.Client.Models
{
    public class SimpleAuthSettings
    {
        public SimpleAuthTokenSettings TokenSettings { get; set; }
        public string Corp { get; set; }
        public string App { get; set; }
        public string Env { get; set; }
        public string Tenant { get; set; }
        public bool LiveChecking { get; set; }
        
        public string SimpleAuthServerUrl { get; set; } = "https://standingtrust.com"; //TODO https
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SimpleAuthTokenSettings
    {
        public string MasterToken { get; set; }
        public string CorpToken { get; set; }
        public string AppToken { get; set; }
        public string[] OtherAppsTokens { get; set; }
        public string OtherAppsSecretTokens { get; set; }
    }
}