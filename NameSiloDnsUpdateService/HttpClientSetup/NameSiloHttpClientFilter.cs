using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;

namespace NameSiloDnsUpdateService.HttpClientSetup
{
    public class NameSiloHttpClientFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory;

        public NameSiloHttpClientFilter(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            return (builder) =>
            {
                next(builder);

                var logger =
                    _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");

                builder.AdditionalHandlers.Insert(0, new NameSiloHttpMessageHandler(logger));
            };
        }
    }
}
