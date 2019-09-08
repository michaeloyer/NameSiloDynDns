using System.Net;

namespace NameSiloDnsUpdateService.NameSilo
{
    public class UpdateHostMessage
    {
        public string RecordID { get; set; }
        public string Host { get; set; }
        public IPAddress IPAddress { get; set; }

    }
}
