using System;
using System.Collections.Specialized;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Shared;

namespace SimpleAuth.Client.Utils
{
    public class RequestBuilder
    {
        public string Url { get; internal set; }
        public bool UseMasterToken { get; set; }
        public bool UseCorpToken { get; set; }
        public bool UseAppToken { get; set; }
        public bool UserFilterEnv { get; set; }
        public bool UseFilterTenant { get; set; }
        public string HttpMethod { get; set; } = Constants.HttpMethods.POST;
        public NameValueCollection QueryParameters { get; set; }
        public string Payload { get; set; }
        public string ContentType { get; set; } = "application/json";

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

        public RequestBuilder WithMasterToken()
        {
            UseMasterToken = true;
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

        public RequestBuilder WithQuery(NameValueCollection queryParameters)
        {
            QueryParameters = queryParameters;
            return this;
        }

        public RequestBuilder WithQuery(string key, string value)
        {
            if (QueryParameters == default)
                QueryParameters = new NameValueCollection();
            QueryParameters[key] = value;
            return this;
        }

        public RequestBuilder WithPayload(string payload)
        {
            Payload = payload;
            return this;
        }

        public RequestBuilder WithoutContentType()
        {
            ContentType = null;
            return this;
        }
    }
}