using System;
using System.Net;
using System.Xml.Serialization;

namespace NameSiloDynDns.NameSilo.ApiModels
{
    public class Request
    {
        [XmlElement("operation")]
        public string Operation { get; set; }
        [XmlElement("ip")]
        public string IPAddress { get; set; }
    }
}
