namespace SimpleAuth.Shared.Domains
{
    public class LocalUserInfo : BaseDomain
    {
        public string Email { get; set; }
        public string Corp { get; set; }
        public string PlainPassword { get; set; }
        public bool Locked { get; set; }
    }
}