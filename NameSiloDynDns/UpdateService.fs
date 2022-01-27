module NameSiloDynDns.Services

open Microsoft.Extensions.Hosting
open NameSiloDynDns.NameSilo
open Serilog
open System
open System.Net
open System.Net.Http
open System.Threading
open System.Threading.Tasks

type UpdateService(repository: NameSiloRepository, hostToUpdate: HostToUpdate, logger: ILogger) =
    inherit BackgroundService()

    let logger = logger.ForContext<UpdateService>()
    let mutable timer = Unchecked.defaultof<Timer>

    override this.ExecuteAsync(stoppingToken) =
        logger.Information("Update Service Starting")
        let checkingPeriod = hostToUpdate.UpdateTimeSpan
        timer <- new Timer(this.CheckForUpdate, stoppingToken, TimeSpan.Zero, checkingPeriod)

        Task.CompletedTask

    override this.StopAsync(stoppingToken) =
        timer.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero)
        |> ignore

        logger.Information("Stopping Update Service")

        base.StopAsync(stoppingToken)

    member this.CheckForUpdate(state: obj) =
        try
            let stoppingToken = state :?> CancellationToken
            stoppingToken.ThrowIfCancellationRequested()
            logger.Debug("Update Service Checking for Change Public IP Address")

            let dnsRecordList =
                repository
                    .GetDnsRecordList()
                    .GetAwaiter()
                    .GetResult()

            dnsRecordList.ResourceRecords
            |> Array.tryFind (fun r ->
                r.Host.Equals(hostToUpdate.Host, StringComparison.InvariantCultureIgnoreCase)
                && r.Type = "A")
            |> function
                | None -> logger.Error("The host {Host} does not exist as an 'A' record", hostToUpdate.Host)
                | Some hostDnsRecord ->
                    let host = hostDnsRecord.Host
                    let hostIP = hostDnsRecord.Value
                    let publicIP = dnsRecordList.CallingIpAddress

                    if not (dnsRecordList.CallingIpAddress.Equals(IPAddress.Parse(hostIP))) then
                        let response =
                            repository
                                .UpdateHostIpAddress(
                                    { RecordID = hostDnsRecord.RecordID
                                      Host = host
                                      IPAddress = publicIP }
                                )
                                .GetAwaiter()
                                .GetResult()

                        logger
                            .ForContext("UpdatedIP", true)
                            .Warning(
                                "New Public IP Address {PublicIP}. Successfully Updated the host {Host} from {HostIP}",
                                publicIP,
                                host,
                                hostIP
                            )
                    else
                        logger
                            .ForContext("UpdatedIP", true)
                            .Information(
                                "Public IP {PublicIP} and Host IP {HostIP} are the same on the host {Host}",
                                publicIP,
                                hostIP,
                                host
                            )
        with
        | :? HttpRequestException as ex -> logger.Error(ex, "HTTP Error to NameSilo")
        | :? NameSiloApiException as ex ->
            logger.Error(ex, $"Namesilo API Error: Operation: {ex.operation}; Code: {ex.code}")
        | ex -> logger.Error(ex, "Generic Error Checking for Update")

    override _.Dispose() = timer.Dispose()
