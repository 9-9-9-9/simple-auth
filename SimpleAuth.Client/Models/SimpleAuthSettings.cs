namespace SimpleAuth.Client.Models
{
    public class SimpleAuthSettings
    {
        public string CorpToken { get; set; }
        public string AppToken { get; set; }
        
        public string SimpleAuthServerUrl { get; set; } = "http://standingtrust.com"; //TODO https
    }
}