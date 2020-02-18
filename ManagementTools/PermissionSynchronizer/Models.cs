using SimpleAuth.Shared.Enums;

namespace PermissionSynchronizer
{
    public class AppTokenModel
    {
        public string Corp { get; set; }
        public string App { get; set; }
        public string AppToken { get; set; }
    }

    public class UserModel
    {
        public string[] PermissionGroups { get; set; }
    }

    public class PermissionRecordModel
    {
        public string Role { get; set; }
        public Verb Verb { get; set; }
    }

    public class PermissionGroupModel
    {
        public PermissionRecordModel[] PermissionRecordModels { get; set; }
    }
}