using System;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared;

namespace SimpleAuth.Client.Utils
{
    public class RequestBuilder
    {
        public string Url { get; private set; }
        public bool UseCorpToken { get; set; }
        public bool UseAppToken { get; set; }
        public bool UserFilterEnv { get; set; }
        public bool UseFilterTenant { get; set; }
        public string HttpMethod { get; set; } = Constants.HttpMethods.POST;

        public RequestBuilder(string url)
        {
            if (url.IsBlank())
                throw new ArgumentNullException(nameof(url));
            Url = url.TrimEnd('/');
        }

        public RequestBuilder Method(string httpMethod)
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

        public RequestBuilder WithFilterEnv()
        {
            UserFilterEnv = true;
            return this;
        }

        public RequestBuilder WithFilterTenant()
        {
            UseFilterTenant = true;
            return this;
        }
    }
}