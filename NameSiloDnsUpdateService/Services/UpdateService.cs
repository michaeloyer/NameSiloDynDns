using Microsoft.Extensions.Hosting;
using NameSiloDnsUpdateService.NameSilo;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NameSiloDnsUpdateService.Services
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
            var checkingPeriod = new TimeSpan(hours: hostToUpdate.Hours, minutes: hostToUpdate.Minutes, seconds: hostToUpdate.Seconds);
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
                logger.Information("Update Service Working");

                var dnsRecordList = repository.GetDnsRecordList().GetAwaiter().GetResult();

                var hostDnsRecord = dnsRecordList.ResourceRecords
                    .FirstOrDefault(r => r.Host.Equals(hostToUpdate.Host, StringComparison.InvariantCultureIgnoreCase)
                        && r.Type == "A");

                if (hostDnsRecord == null)
                {
                    logger.Error($"The host {hostToUpdate.Host} does not exist as an 'A' record");
                    return;
                }

                if (!dnsRecordList.CallingIpAddress.Equals(IPAddress.Parse(hostDnsRecord.Value)))
                {
                    var response = repository.UpdateHostIpAddress(new UpdateHostMessage
                    {
                        RecordID = hostDnsRecord.RecordID,
                        Host = hostDnsRecord.Host,
                        IPAddress = dnsRecordList.CallingIpAddress
                    }).GetAwaiter().GetResult();

                    logger.Warning($"New Public IP Address. Successfully Updated the host '{hostDnsRecord.Host}' " +
                        $"to IP Address {dnsRecordList.CallingIpAddress}");
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
            finally
            {
            }
        }

        public override void Dispose()
        {
            timer.Dispose();
        }
    }
}
