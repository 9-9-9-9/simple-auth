using System;
using System.Net.Http;
using SimpleAuth.Core.Extensions;

namespace SimpleAuth.Client.Utils
{
    public class RequestBuilder
    {
        public string Url { get; private set; }
        public bool UseCorpToken { get; set; }
        public bool UseAppToken { get; set; }
        public HttpMethod HttpMethod { get; set; } = HttpMethod.Post;

        public RequestBuilder(string url)
        {
            if (url.IsBlank())
                throw new ArgumentNullException(nameof(url));
            Url = url.TrimEnd('/');
        }

        public RequestBuilder Method(HttpMethod httpMethod)
        {
            HttpMethod = httpMethod;
            return this;
        }

        public RequestBuilder Append(string path)
        {
            if (!path.IsBlank())
                Url += $"/{path.TrimStart('/')}";
            return this;
        }

        public RequestBuilder WithCorpToken()
        {
            UseCorpToken = true;
            return this;
        }

        public RequestBuilder WithAppToken()
        {
            UseAppToken = true;
            return this;
        }
    }
}