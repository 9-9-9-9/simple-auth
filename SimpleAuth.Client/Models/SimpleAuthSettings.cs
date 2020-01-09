namespace SimpleAuth.Client.Models
{
    public class SimpleAuthSettings
    {
        public SimpleAuthTokenSettings TokenSettings { get; set; }
        public string Corp { get; set; }
        public string App { get; set; }
        public string Env { get; set; }
        public string Tenant { get; set; }
        
        public string SimpleAuthServerUrl { get; set; } = "http://standingtrust.com"; //TODO https
    }

    public class SimpleAuthTokenSettings
    {
        public string CorpToken { get; set; }
        public string AppToken { get; set; }
    }
}