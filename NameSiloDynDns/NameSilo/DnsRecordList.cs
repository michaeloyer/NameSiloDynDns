using System.Net;

namespace NameSiloDynDns.NameSilo
{
    public class DnsRecordList
    {
        public IPAddress CallingIpAddress { get; set; }
        public DnsRecord[] ResourceRecords;
    }
}
