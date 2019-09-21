using System.Net.Http;

namespace NameSiloDnsUpdateService
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
