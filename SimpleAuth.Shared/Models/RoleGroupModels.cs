using System.ComponentModel.DataAnnotations;
using System.Linq;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Domains;

namespace SimpleAuth.Shared.Models
{
    public class CreateRoleGroupModel
    {
        public string Name { get; set; }

        [Required] public string Corp { get; set; }

        [Required] public string App { get; set; }

        public string[] CopyFromRoleGroups { get; set; }
    }

    public class RoleGroupResponseModel
    {
        public string Name { get; set; }
        public PermissionModel[] Roles { get; set; }

        public static RoleGroupResponseModel Cast(RoleGroup group)
        {
            return new RoleGroupResponseModel
            {
                Name = group.Name,
                Roles = (group.Roles?.Select(PermissionModel.Cast)).OrEmpty().ToArray()
            };
        }
    }
}