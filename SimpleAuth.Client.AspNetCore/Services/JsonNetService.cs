using Newtonsoft.Json;
using SimpleAuth.Client.Services;

namespace SimpleAuth.Client.AspNetCore.Services
{
    public class JsonNetService : IJsonService
    {
        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}