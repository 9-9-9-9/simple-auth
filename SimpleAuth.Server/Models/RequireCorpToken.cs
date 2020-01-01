namespace SimpleAuth.Server.Models
{
    public class RequireCorpToken
    {
        public string Header { get; set; }
        public string Corp { get; set; }
        public int Version { get; set; }
    }
}