using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NameSiloDynDns.HttpClientSetup
{
    public class NameSiloHttpMessageHandler : DelegatingHandler
    {
        private ILogger _logger;

        public NameSiloHttpMessageHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Log.RequestStart(_logger, request);
            var response = await base.SendAsync(request, cancellationToken);
            Log.RequestEnd(_logger, response);

            return response;
        }

        private static class Log
        {
            public static class EventIds
            {
                public static readonly EventId RequestStart = new EventId(100, "RequestStart");
                public static readonly EventId RequestEnd = new EventId(101, "RequestEnd");
            }

            private static readonly Action<ILogger, HttpMethod, string, Exception> _requestStart = LoggerMessage.Define<HttpMethod, string>(
                LogLevel.Information,
                EventIds.RequestStart,
                "Sending HTTP request {HttpMethod} {Uri}");

            private static readonly Action<ILogger, HttpStatusCode, Exception> _requestEnd = LoggerMessage.Define<HttpStatusCode>(
                LogLevel.Information,
                EventIds.RequestEnd,
                "Received HTTP response {StatusCode}");

            public static void RequestStart(ILogger logger, HttpRequestMessage request)
            {
                _requestStart(logger, request.Method, Regex.Replace(request.RequestUri.ToString(), @"key=\w+", "key=REDACTED"), null);
            }

            public static void RequestEnd(ILogger logger, HttpResponseMessage response)
            {
                _requestEnd(logger, response.StatusCode, null);
            }
        }
    }
}