using System.ComponentModel.DataAnnotations;
using System.Linq;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Shared.Domains;

namespace SimpleAuth.Shared.Models
{
    public class CreatePermissionGroupModel
    {
        public string Name { get; set; }
        
        public string Description { get; set; }

        [Required] public string Corp { get; set; }

        [Required] public string App { get; set; }

        public string[] CopyFromPermissionGroups { get; set; }
    }

    public class UpdatePermissionGroupInfoModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class PermissionGroupResponseModel
    {
        public string Name { get; set; }
        public PermissionModel[] Roles { get; set; }

        public static PermissionGroupResponseModel Cast(PermissionGroup group)
        {
            return new PermissionGroupResponseModel
            {
                Name = group.Name,
                Roles = (group.Permissions?.Select(PermissionModel.Cast)).OrEmpty().ToArray()
            };
        }
    }
}