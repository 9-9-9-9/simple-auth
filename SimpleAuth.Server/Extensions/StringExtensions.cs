using System.Text.Json;

namespace SimpleAuth.Server.Extensions
{
    /// <summary>
    /// Extensions for working with string
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Serialize object into string
        /// </summary>
        /// <param name="obj">Object to be serialized</param>
        /// <typeparam name="T">Type of object to be serialized</typeparam>
        /// <returns>Json serialized object</returns>
        public static string ToJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Deserialize object from string
        /// </summary>
        /// <param name="jsonData">Json string of the serialized object</param>
        /// <typeparam name="T">Type of the original object</typeparam>
        /// <returns>Object which were deserialized from string</returns>
        public static T FromJson<T>(this string jsonData)
        {
            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}