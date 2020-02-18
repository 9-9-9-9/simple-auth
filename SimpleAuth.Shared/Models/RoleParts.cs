using SimpleAuth.Shared.Enums;

namespace SimpleAuth.Shared.Models
{
    public interface IRolePart
    {
    }

    public interface ICorpRelated : IRolePart
    {
        string Corp { get; set; }
    }

    public interface IAppRelated : IRolePart
    {
        string App { get; set; }
    }

    public interface IEnvRelated : IRolePart
    {
        string Env { get; set; }
    }

    public interface ITenantRelated : IRolePart
    {
        string Tenant { get; set; }
    }

    public interface IModuleRelated : IRolePart
    {
        string Module { get; set; }
    }

    public interface ISubModuleRelated : IRolePart
    {
        string SubModules { get; set; }
    }

    public interface IPermissionRelated
    {
        Verb Verb { get; set; }
    }

    public interface ILockable
    {
        bool Locked { get; set; }
    }
}