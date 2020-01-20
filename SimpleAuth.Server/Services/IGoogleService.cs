using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Server.Services
{
    /// <summary>
    /// Services for communicating with Google
    /// </summary>
    public interface IGoogleService
    {
        // ReSharper disable once CommentTypo
        /// <summary>
        /// Connect to tokeninfo endpoint of Google OAuth2 api to retrieve information
        /// </summary>
        /// <param name="token">Token string to forward to Google OAuth</param>
        /// <returns>Model response by Google OAuth</returns>
        Task<GoogleTokenResponseResult> GetInfoAsync(string token);
    }

    /// <summary>
    /// The first and default implementation
    /// </summary>
    public class DefaultGoogleService : IGoogleService
    {
        /// <inheritdoc />
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