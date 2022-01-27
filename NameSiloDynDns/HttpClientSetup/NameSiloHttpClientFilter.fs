namespace NameSiloDynDns.HttpClientSetup

open Microsoft.Extensions.Http
open Microsoft.Extensions.Logging
open System

type NameSiloHttpClientFilter(loggerFactory: ILoggerFactory) =
    interface IHttpMessageHandlerBuilderFilter with
        member this.Configure(next) =
            if isNull next then
                nullArg (nameof next)
            else
                Action<HttpMessageHandlerBuilder> (fun builder ->
                    next.Invoke builder

                    let logger =
                        loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler")

                    builder.AdditionalHandlers.Insert(0, new NameSiloHttpMessageHandler(logger)))
