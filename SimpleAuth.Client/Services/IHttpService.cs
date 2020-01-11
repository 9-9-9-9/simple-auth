using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using SimpleAuth.Client.Exceptions;
using SimpleAuth.Client.InternalExtensions;
using SimpleAuth.Client.Utils;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Client.Services
{
    public interface IHttpService
    {
        Task<(bool, HttpStatusCode, TResult)> DoHttpRequestAsync<TResult>(
            RequestBuilder requestBuilder,
            string payload = null);

        Task<(bool, HttpStatusCode)> DoHttpRequestWithoutResponseAsync(
            RequestBuilder requestBuilder,
            string payload = null);

        Task DoHttpRequestWithoutResponseAsync(
            bool expectSuccessStatusCode,
            RequestBuilder requestBuilder,
            string payload = null);

        Task<TResult> DoHttpRequestWithResponseContentAsync<TResult>(
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
                    Constants.HttpMethods.GET == requestBuilder.HttpMethod
                    ||
                    Constants.HttpMethods.DELETE == requestBuilder.HttpMethod
                )
                {
                    throw new NotSupportedException(
                        $"Http method {requestBuilder.HttpMethod} does not supports '{nameof(payload)}' parameter");
                }
            }

            using var httpClient = NewHttpClient(requestBuilder);

            var response = await DoRequest(httpClient, requestBuilder, payload);

            TResult responseContent = default;
            if (response.IsSuccessStatusCode)
            {
                var responseContentString = await response.Content.ReadAsStringAsync();
                if (responseContentString != null)
                {
                    if (typeof(TResult) == typeof(string))
                        responseContent = (TResult) (object) responseContentString;
                    else
                    {
                        try
                        {
                            responseContent = responseContentString.JsonDeserialize<TResult>();
                        }
                        catch (Exception e)
                        {
                            throw new SimpleAuthException($"Can not deserialize json '{responseContentString}'", e);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    var responseContentString = await response.Content.ReadAsStringAsync();
                    if (responseContentString != null)
                        $"Response error with message: {responseContentString}".Write();
                }
                catch (Exception e)
                {
#if DEBUG
                    $"Unable to try reading response with err message: {e.Message}".Write();
#endif
                }
            }

            return (response.IsSuccessStatusCode, response.StatusCode, responseContent);
        }

        private async Task<HttpResponseMessage> DoRequest(HttpClient httpClient, RequestBuilder requestBuilder,
            string payload)
        {
            HttpContent httpContent =
                payload == null ? null : new StringContent(payload, Encoding.UTF8, "application/json");

            var requestUrl = requestBuilder.Url;
            HttpResponseMessage response;

#if DEBUG
            $"Requesting {requestUrl}".Write();
#endif

            if (Constants.HttpMethods.GET == requestBuilder.HttpMethod)
                response = await httpClient.GetAsync(requestUrl);
            else if (Constants.HttpMethods.POST == requestBuilder.HttpMethod)
                response = await httpClient.PostAsync(requestUrl, httpContent);
            else if (Constants.HttpMethods.PUT == requestBuilder.HttpMethod)
                response = await httpClient.PutAsync(requestUrl, httpContent);
            else if (Constants.HttpMethods.DELETE == requestBuilder.HttpMethod)
                response = await httpClient.DeleteAsync(requestUrl);
            else
                throw new NotSupportedException(
                    $"{requestBuilder.HttpMethod} is not being supported by this function <{nameof(DoHttpRequestAsync)}>");

#if DEBUG
            $"Request completed ({response.IsSuccessStatusCode} {response.StatusCode})".Write();
#endif
            return response;
        }

        public async Task<(bool, HttpStatusCode)> DoHttpRequestWithoutResponseAsync(RequestBuilder requestBuilder,
            string payload = null)
        {
            if (requestBuilder == null)
                throw new ArgumentNullException(nameof(requestBuilder));

            if (!payload.IsBlank())
            {
                if (
                    Constants.HttpMethods.GET == requestBuilder.HttpMethod
                    ||
                    Constants.HttpMethods.DELETE == requestBuilder.HttpMethod
                )
                {
                    throw new NotSupportedException(
                        $"Http method {requestBuilder.HttpMethod} does not supports '{nameof(payload)}' parameter");
                }
            }

            using var httpClient = NewHttpClient(requestBuilder);

            var response = await DoRequest(httpClient, requestBuilder, payload);

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var responseContentString = await response.Content.ReadAsStringAsync();
                    if (responseContentString != null)
                        $"Response error with message: {responseContentString}".Write();
                }
                catch (Exception e)
                {
#if DEBUG
                    $"Unable to try reading response with err message: {e.Message}".Write();
#endif
                }
            }

            return (response.IsSuccessStatusCode, response.StatusCode);
        }

        public async Task DoHttpRequestWithoutResponseAsync(bool expectSuccessStatusCode, RequestBuilder requestBuilder,
            string payload = null)
        {
            var (success, httpStatusCode) = await DoHttpRequestWithoutResponseAsync(requestBuilder, payload);
            if (!success)
                throw new SimpleAuthHttpRequestException(httpStatusCode);
        }

        public async Task<TResult> DoHttpRequestWithResponseContentAsync<TResult>(RequestBuilder requestBuilder,
            string payload = null)
        {
            var (success, httpStatusCode, result) = await DoHttpRequestAsync<TResult>(requestBuilder, payload);
            if (!success)
                throw new SimpleAuthHttpRequestException(httpStatusCode);
            return result;
        }

        private HttpClient NewHttpClient(RequestBuilder requestBuilder)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (requestBuilder.UseMasterToken)
                httpClient.DefaultRequestHeaders.Add(Constants.Headers.MasterToken,
                    _simpleAuthConfigurationProvider.MasterToken);

            if (requestBuilder.UseCorpToken)
                httpClient.DefaultRequestHeaders.Add(Constants.Headers.CorpPermission,
                    _simpleAuthConfigurationProvider.CorpToken);

            if (requestBuilder.UseAppToken)
                httpClient.DefaultRequestHeaders.Add(Constants.Headers.AppPermission,
                    _simpleAuthConfigurationProvider.AppToken);

            if (requestBuilder.UserFilterEnv)
                httpClient.DefaultRequestHeaders.Add(Constants.Headers.FilterByEnv,
                    _simpleAuthConfigurationProvider.Env);

            if (requestBuilder.UseFilterTenant)
                httpClient.DefaultRequestHeaders.Add(Constants.Headers.FilterByTenant,
                    _simpleAuthConfigurationProvider.Tenant);

            httpClient.Timeout = TimeSpan.FromMinutes(5);

            if (!requestBuilder.QueryParameters.IsEmpty())
            {
                int? port = null;
                if (Regex.IsMatch(requestBuilder.Url, @"\:\d+[\/\?]?"))
                {
                    var uri = new Uri(requestBuilder.Url);
                    port = uri.Port;
                }

                var builder = new UriBuilder(requestBuilder.Url) {Port = port ?? -1};
                var query = HttpUtility.ParseQueryString(builder.Query);
                requestBuilder.QueryParameters.ToList().ForEach(kvp => query[kvp.Key] = kvp.Value);
                builder.Query = query.ToString();
                requestBuilder.Url = builder.ToString();
            }

            return httpClient;
        }
    }
}