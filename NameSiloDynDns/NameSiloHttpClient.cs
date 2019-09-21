using System.Net.Http;

namespace NameSiloDynDns
{
    public class NameSiloHttpClient
    {
        public NameSiloHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }
    }
}
