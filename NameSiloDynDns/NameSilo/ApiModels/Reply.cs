using System.Xml.Serialization;

namespace NameSiloDynDns.NameSilo.ApiModels
{
    public class Reply
    {
        [XmlElement("code")]
        public string Code { get; set; }
        [XmlElement("detail")]
        public string Detail { get; set; }
        [XmlElement("resource_record")]
        public ResourceRecord[] ResourceRecords { get; set; }
        [XmlElement("record_id")]
        public string RecordID { get; set; }
    }
}