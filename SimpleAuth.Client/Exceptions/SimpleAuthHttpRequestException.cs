using System.Net;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Client.Exceptions
{
    public class SimpleAuthHttpRequestException : SimpleAuthException
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public SimpleAuthHttpRequestException(HttpStatusCode statusCode) : base(BuildErrorMessage(statusCode))
        {
            HttpStatusCode = statusCode;
        }

        private static string BuildErrorMessage(HttpStatusCode statusCode)
        {
            //TODO handle cases
            return $"Request failure, status code {statusCode} ({(byte)statusCode})";
        }
    }
}