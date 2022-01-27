namespace NameSiloDynDns.HttpClientSetup

open Microsoft.Extensions.Logging
open System.Net
open System.Net.Http
open System.Text.RegularExpressions

module private Log =
    module EventIds =
        let RequestStart = EventId(100, "RequestStart")
        let RequestEnd = EventId(101, "RequestEnd")

    let private requestStartAction =
        LoggerMessage.Define<HttpMethod, string>(
            LogLevel.Information,
            EventIds.RequestStart,
            "Sending HTTP request {HttpMethod} {Uri}"
        )

    let private requestEndAction =
        LoggerMessage.Define<HttpStatusCode>(
            LogLevel.Information,
            EventIds.RequestEnd,
            "Received HTTP response {StatusCode}"
        )

    let requestStart logger (request: HttpRequestMessage) =
        requestStartAction.Invoke(
            logger,
            request.Method,
            Regex.Replace(request.RequestUri.ToString(), @"key=\w+", "key=REDACTED"),
            null
        )

    let requestEnd logger (response: HttpResponseMessage) =
        requestEndAction.Invoke(logger, response.StatusCode, null)

type NameSiloHttpMessageHandler(logger: ILogger) =
    inherit DelegatingHandler()

    member this.SendAsyncProtected(request, cancellationToken) =
        base.SendAsync(request, cancellationToken)

    override this.SendAsync(request, cancellationToken) =
        if isNull request then
            nullArg (nameof request)
        else
            task {
                Log.requestStart logger request
                let! response = this.SendAsyncProtected(request, cancellationToken)
                Log.requestEnd logger response

                return response
            }
