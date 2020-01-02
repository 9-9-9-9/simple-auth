using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SimpleAuth.Client.Exceptions;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;

namespace SimpleAuth.Client.Services
{
    public interface IHttpService
    {
        Task<(bool, HttpStatusCode, TResult)> DoHttpRequestAsync<TResult>(
            RequestBuilder requestBuilder,
            string payload = null);
        
        Task<TResult> DoHttpRequest2Async<TResult>(
            RequestBuilder requestBuilder,
            string payload = null);
    }

    public class DefaultHttpService : IHttpService
    {
        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;

        public DefaultHttpService(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider)
        {
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
        }

        public async Task<(bool, HttpStatusCode, TResult)> DoHttpRequestAsync<TResult>(
            RequestBuilder requestBuilder,
            string payload = null)
        {
            if (requestBuilder == null)
                throw new ArgumentNullException(nameof(requestBuilder));

            if (!payload.IsBlank())
            {
                if (
                    HttpMethod.Get == requestBuilder.HttpMethod
                    ||
                    HttpMethod.Delete == requestBuilder.HttpMethod
                )
                {
                    throw new NotSupportedException(
                        $"Http method {requestBuilder.HttpMethod} does not supports '{nameof(payload)}' parameter");
                }
            }

            using var httpClient = NewHttpClient(requestBuilder);
            HttpContent httpContent =
                payload == null ? null : new StringContent(payload, Encoding.UTF8, "application/json");

            var requestUrl = requestBuilder.Url;
            HttpResponseMessage response;

            if (HttpMethod.Get == requestBuilder.HttpMethod)
                response = await httpClient.GetAsync(requestUrl);
            else if (HttpMethod.Post == requestBuilder.HttpMethod)
                response = await httpClient.PostAsync(requestUrl, httpContent);
            else if (HttpMethod.Put == requestBuilder.HttpMethod)
                response = await httpClient.PutAsync(requestUrl, httpContent);
            else if (HttpMethod.Patch == requestBuilder.HttpMethod)
                response = await httpClient.PatchAsync(requestUrl, httpContent);
            else if (HttpMethod.Delete == requestBuilder.HttpMethod)
                response = await httpClient.DeleteAsync(requestUrl);
            else
                throw new NotSupportedException(
                    $"{requestBuilder.HttpMethod.Method} is not being supported by this function <{nameof(DoHttpRequestAsync)}>");

            TResult responseContent = default;
            if (response.IsSuccessStatusCode)
            {
                var responseContentString = await response.Content.ReadAsStringAsync();
                if (responseContentString != null)
                    responseContent = responseContentString.JsonDeserialize<TResult>();
            }

            return (response.IsSuccessStatusCode, response.StatusCode, responseContent);
        }

        public async Task<TResult> DoHttpRequest2Async<TResult>(RequestBuilder requestBuilder, string payload = null)
        {
            var res = await DoHttpRequestAsync<TResult>(requestBuilder, payload);
            if (res.Item1)
                return res.Item3;
            throw new SimpleAuthHttpRequestException(res.Item2);
        }

        private HttpClient NewHttpClient(RequestBuilder requestBuilder)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (requestBuilder.UseCorpToken)
                httpClient.DefaultRequestHeaders.Add(Constants.Headers.CorpPermission,
                    _simpleAuthConfigurationProvider.CorpToken);
            
            if (requestBuilder.UseAppToken)
                httpClient.DefaultRequestHeaders.Add(Constants.Headers.AppPermission,
                    _simpleAuthConfigurationProvider.AppToken);

            httpClient.Timeout = TimeSpan.FromMinutes(5);
            return httpClient;
        }
    }
}