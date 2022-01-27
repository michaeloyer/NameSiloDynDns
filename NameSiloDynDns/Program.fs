module NameSiloDynDns.Program

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open NameSiloDynDns.HttpClientSetup
open NameSiloDynDns.Services
open Polly
open Polly.Extensions.Http
open Serilog
open System

[<EntryPoint>]
let main args =
    let builder = Host.CreateDefaultBuilder(args)

    builder.UseSerilog(
        Action<HostBuilderContext, LoggerConfiguration> (fun host config ->
            config.ReadFrom.Configuration(host.Configuration)
            |> ignore)
    )
    |> ignore

    let includePrivateProperties (options: BinderOptions) = options.BindNonPublicProperties <- true

    builder.ConfigureServices(
        Action<HostBuilderContext, IServiceCollection> (fun host services ->
            let hostToUpdateConfiguration =
                host
                    .Configuration
                    .GetSection("HostToUpdate")
                    .Get<HostToUpdate>(includePrivateProperties)

            services
                .AddHttpClient<NameSiloHttpClient>()
                .ConfigureNameSiloHttpLogging()
                .AddPolicyHandler(
                    HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(
                            hostToUpdateConfiguration.RetryAttempts,
                            fun retryAttempt -> hostToUpdateConfiguration.RetryTimeSpan
                        )
                )
            |> ignore

            services
                .AddSingleton(
                    host
                        .Configuration
                        .GetSection("NameSiloApi")
                        .Get<ApiConfiguration>(includePrivateProperties)
                )
                .AddSingleton(hostToUpdateConfiguration)
                .AddTransient<NameSiloRepository>()
                .AddHostedService<UpdateService>()
            |> ignore

            ())
    )
    |> ignore

    builder.RunConsoleAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously

    0
