using Newtonsoft.Json;

namespace SimpleAuth.Client.InternalExtensions
{
    internal static class InternalJsonExtensions
    {
        public static string JsonSerialize<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T JsonDeserialize<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}