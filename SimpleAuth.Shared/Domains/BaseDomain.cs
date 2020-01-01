using SimpleAuth.Shared.Models;

namespace SimpleAuth.Shared.Domains
{
    public abstract class BaseDomain
    {
    }

    public interface IRawSubModulesRelated : IRolePart
    {
        string[] SubModules { get; set; }
    }
}