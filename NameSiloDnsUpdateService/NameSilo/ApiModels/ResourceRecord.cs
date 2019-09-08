using System.Xml.Serialization;

namespace NameSiloDnsUpdateService.NameSilo.ApiModels
{
    public class ResourceRecord
    {
        [XmlElement("record_id")]
        public string RecordID { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("host")]
        public string Host { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }

        [XmlElement("ttl")]
        public int TimeToLive { get; set; }

        [XmlElement("distance")]
        public int Distance { get; set; }
    }
}
