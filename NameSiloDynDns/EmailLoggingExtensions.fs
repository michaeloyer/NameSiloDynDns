module NameSiloDynDns.EmailLoggingExtensions

open Serilog
open Serilog.Configuration
open Serilog.Events
open Serilog.Formatting
open Serilog.Sinks.Email
open System
open System.Net

type MyEmailConnectionInfo() =
    inherit EmailConnectionInfo()

    member _.NetworkCredentials
        with get () = base.NetworkCredentials :?> NetworkCredential
        and set (value: NetworkCredential) = base.NetworkCredentials <- value

type LoggerSinkConfiguration with
    member loggerConfiguration.Email
        (
            myConnectionInfo: MyEmailConnectionInfo,
            ?outputTemplate: string,
            ?restrictedToMinimumLevel: LogEventLevel,
            ?batchPostingLimit: int,
            ?period: TimeSpan,
            ?formatProvider: IFormatProvider,
            ?mailSubject: string
        ) =
        loggerConfiguration.Email(
            myConnectionInfo :> EmailConnectionInfo,
            defaultArg outputTemplate "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
            defaultArg restrictedToMinimumLevel LogEventLevel.Verbose,
            defaultArg batchPostingLimit 100,
            Option.toNullable<TimeSpan> period,
            Option.toObj<IFormatProvider> formatProvider,
            defaultArg mailSubject "Log Email"
        )

    member loggerConfiguration.Email
        (
            myConnectionInfo: MyEmailConnectionInfo,
            textFormatter: ITextFormatter,
            ?restrictedToMinimumLevel,
            ?batchPostingLimit,
            ?period,
            ?mailSubject
        ) =
        loggerConfiguration.Email(
            myConnectionInfo :> EmailConnectionInfo,
            textFormatter,
            defaultArg restrictedToMinimumLevel LogEventLevel.Verbose,
            defaultArg batchPostingLimit 100,
            Option.toNullable<TimeSpan> period,
            defaultArg mailSubject "Log Email"
        )
