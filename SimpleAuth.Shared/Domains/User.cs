namespace SimpleAuth.Shared.Domains
{
    public class User : BaseDomain
    {
        public string Id { get; set; }
        public PermissionGroup[] PermissionGroups { get; set; }
        public LocalUserInfo[] LocalUserInfos { get; set; }
    }
}