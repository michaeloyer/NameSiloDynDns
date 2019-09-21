using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace NameSiloDnsUpdateService.HttpClientSetup
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
