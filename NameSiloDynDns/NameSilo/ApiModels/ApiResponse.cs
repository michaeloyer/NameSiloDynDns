using System.Xml.Serialization;

namespace NameSiloDynDns.NameSilo.ApiModels
{
    [XmlRoot("namesilo")]
    public class ApiResponse
    {
        [XmlElement("request")]
        public Request Request { get; set; }
        [XmlElement("reply")]
        public Reply Reply { get; set; }

        public bool IsApiSuccessful => Reply.Code == "300";

        public void EnsureSuccessfulResponseCode()
        {
            if (IsApiSuccessful == false)
                throw new NameSiloApiException(this);
        }
    }
}
