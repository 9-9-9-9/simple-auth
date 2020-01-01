using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Server.Services
{
    public interface IGoogleService
    {
        Task<GoogleTokenResponseResult> GetInfoAsync(string token);
    }

    public class DefaultGoogleService : IGoogleService
    {
        public async Task<GoogleTokenResponseResult> GetInfoAsync(string token)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };

            var httpResponse =
                await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={token}");

            if (!httpResponse.IsSuccessStatusCode)
                throw new SimpleAuthException(httpResponse.StatusCode.ToString());

            var response = await httpResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoogleTokenResponseResult>(response);
        }
    }
}