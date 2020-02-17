namespace SimpleAuth.Shared.Domains
{
    public class User : BaseDomain
    {
        public string Id { get; set; }
        public PermissionGroup[] RoleGroups { get; set; }
        public LocalUserInfo[] LocalUserInfos { get; set; }
    }
}