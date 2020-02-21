using System.Net;
using SimpleAuth.Shared.Extensions;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Client.Exceptions
{
    public class SimpleAuthHttpRequestException : SimpleAuthException
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string DescribeMessage { get; set; }
        
        public SimpleAuthHttpRequestException(HttpStatusCode statusCode, string describeMessage = null) : base(BuildErrorMessage(statusCode, describeMessage))
        {
            HttpStatusCode = statusCode;
            DescribeMessage = describeMessage;
        }

        private static string BuildErrorMessage(HttpStatusCode statusCode, string describeMessage)
        {
            //TODO handle cases
            return $"Request failure, status code {statusCode} ({(short)statusCode}){(describeMessage.IsBlank()? "" : $", reason: '{describeMessage}'")}";
        }
    }
}