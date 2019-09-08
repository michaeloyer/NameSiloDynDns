using System.Net;

namespace NameSiloDnsUpdateService.NameSilo
{
    public class DnsRecordList
    {
        public IPAddress CallingIpAddress { get; set; }
        public DnsRecord[] ResourceRecords;
    }
}
