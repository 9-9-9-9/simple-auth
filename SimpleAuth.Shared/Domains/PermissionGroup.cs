using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Domains
{
    public class PermissionGroup : BaseDomain, ICorpRelated, IAppRelated, ILockable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Permission[] Permissions { get; set; }
        
        public string Corp { get; set; }
        
        public string App { get; set; }
        
        public bool Locked { get; set; }
    }
}