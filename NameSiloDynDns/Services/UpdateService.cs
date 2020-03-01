using Microsoft.Extensions.Hosting;
using NameSiloDynDns.NameSilo;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NameSiloDynDns.Services
{
    public class UpdateService : BackgroundService
    {
        private Timer timer;
        private readonly ILogger logger;
        private readonly NameSiloRepository repository;
        private readonly HostToUpdate hostToUpdate;

        public UpdateService(NameSiloRepository repository, HostToUpdate hostToUpdate, ILogger logger)
        {
            this.logger = logger.ForContext<UpdateService>();
            this.repository = repository;
            this.hostToUpdate = hostToUpdate;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.Information("Update Service Starting");
            var checkingPeriod = hostToUpdate.UpdateTimeSpan;
            timer = new Timer(CheckForUpdate, stoppingToken, TimeSpan.Zero, checkingPeriod);

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero);
            logger.Information("Stopping Update Service");

            return base.StopAsync(cancellationToken);
        }

        private void CheckForUpdate(object state)
        {
            try
            {
                var stoppingToken = (CancellationToken)state;
                stoppingToken.ThrowIfCancellationRequested();
                logger.Debug("Update Service Checking for Change Public IP Address");

                var dnsRecordList = repository.GetDnsRecordList().GetAwaiter().GetResult();

                var hostDnsRecord = dnsRecordList.ResourceRecords
                    .FirstOrDefault(r => r.Host.Equals(hostToUpdate.Host, StringComparison.InvariantCultureIgnoreCase)
                        && r.Type == "A");

                if (hostDnsRecord == null)
                {
                    logger.Error("The host {Host} does not exist as an 'A' record", hostToUpdate.Host);
                    return;
                }

                var host = hostDnsRecord.Host;
                var hostIP = hostDnsRecord.Value;
                var publicIP = dnsRecordList.CallingIpAddress;

                if (!dnsRecordList.CallingIpAddress.Equals(IPAddress.Parse(hostIP)))
                {
                    var response = repository.UpdateHostIpAddress(new UpdateHostMessage
                    {
                        RecordID = hostDnsRecord.RecordID,
                        Host = host,
                        IPAddress = publicIP
                    }).GetAwaiter().GetResult();

                    logger.ForContext(propertyName: "UpdatedIP", true)
                        .Warning("New Public IP Address {PublicIP}. Successfully Updated the host {Host} from {HostIP}",
                        publicIP, host, hostIP);
                }
                else
                {
                    logger.ForContext(propertyName: "UpdatedIP", true)
                        .Information("Public IP {PublicIP} and Host IP {HostIP} are the same on the host {Host}",
                        publicIP, hostIP, host);
                }
            }
            catch (HttpRequestException ex)
            {
                logger.Error(ex, $"HTTP Error to NameSilo");
            }
            catch (NameSiloApiException ex)
            {
                logger.Error(ex, $"Namesilo API Error: Operation: {ex.Operation}; Code: {ex.Code}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Generic Error Checking for Update");
            }
        }

        public override void Dispose()
        {
            timer.Dispose();
        }
    }
}
