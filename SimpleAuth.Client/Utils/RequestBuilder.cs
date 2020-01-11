using System;
using System.Collections.Generic;
using SimpleAuth.Core.Extensions;
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
        public IDictionary<string, string> QueryParameters { get; set; }
        public string Payload { get; set; }

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

        public RequestBuilder WithQuery(IDictionary<string, string> queryParameters)
        {
            QueryParameters = queryParameters;
            return this;
        }

        public RequestBuilder WithPayload(string payload)
        {
            Payload = payload;
            return this;
        }
    }
}