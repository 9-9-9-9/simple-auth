using SimpleAuth.Repositories;

namespace Test.SimpleAuth.Shared.Mock.Repositories
{
    public interface IDummyMemoryCachedRepository : IMemoryCachedRepository<string>
    {
    }
    
    public class DummyCachedRepositories : MemoryCachedRepository<string>, IDummyMemoryCachedRepository
    {
    }
}