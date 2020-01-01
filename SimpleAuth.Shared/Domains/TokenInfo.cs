using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Domains
{
    public class TokenInfo : BaseDomain, ICorpRelated, IAppRelated
    {
        public string Corp { get; set; }
        public string App { get; set; }
        public int Version { get; set; }
    }
}