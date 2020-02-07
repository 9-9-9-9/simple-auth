using System;
using System.Net.Http;

#pragma warning disable 1591

namespace SimpleAuth.Server.Services
{
    public interface IHttpService
    {
        HttpClient GetClient();
    }

    public class DefaultHttpService : IHttpService
    {
        public HttpClient GetClient() => new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
    }
}