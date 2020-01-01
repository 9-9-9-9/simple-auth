using SimpleAuth.Shared.Enums;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace SimpleAuth.Shared.Models
{
    public interface IRolePart
    {
    }

    public interface ICorpRelated : IRolePart
    {
        [Index] string Corp { get; set; }
    }

    public interface IAppRelated : IRolePart
    {
        [Index] string App { get; set; }
    }

    public interface IEnvRelated : IRolePart
    {
        [Index] string Env { get; set; }
    }

    public interface ITenantRelated : IRolePart
    {
        [Index] string Tenant { get; set; }
    }

    public interface IModuleRelated : IRolePart
    {
        [Index] string Module { get; set; }
    }

    public interface ISubModuleRelated : IRolePart
    {
        [Index] string SubModules { get; set; }
    }

    public interface IPermissionRelated
    {
        [Index] Permission Permission { get; set; }
    }

    public interface ILockable
    {
        [Index] bool Locked { get; set; }
    }
}