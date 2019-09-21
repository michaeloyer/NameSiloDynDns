using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace NameSiloDynDns.HttpClientSetup
{
    public static class HttpClientExtensions
    {
        public static IHttpClientBuilder ConfigureNameSiloHttpLogging(this IHttpClientBuilder builder)
        {
            builder.Services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, NameSiloHttpClientFilter>());

            return builder;
        }

    }
}
