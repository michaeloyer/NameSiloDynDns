using System.Net;

namespace NameSiloDynDns.NameSilo
{
    public class UpdateHostMessage
    {
        public string RecordID { get; set; }
        public string Host { get; set; }
        public IPAddress IPAddress { get; set; }

    }
}
