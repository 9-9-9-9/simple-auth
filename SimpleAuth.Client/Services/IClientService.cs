using SimpleAuth.Client.Utils;

namespace SimpleAuth.Client.Services
{
    public interface IClientService
    {
    }

    public abstract class ClientService : IClientService
    {
        protected readonly ISimpleAuthConfigurationProvider SimpleAuthConfigurationProvider;

        protected ClientService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            SimpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        protected virtual RequestBuilder NewRequest()
        {
            return new RequestBuilder(SimpleAuthConfigurationProvider.EndPointUrl);
        }
    }
}