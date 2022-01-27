[<AutoOpen>]
module NameSiloDynDns.HttpClientSetup.Extensions

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Http

type IHttpClientBuilder with
    member builder.ConfigureNameSiloHttpLogging() =
        builder.Services.Replace(
            ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, NameSiloHttpClientFilter>()
        )
        |> ignore

        builder
