using MoreLinq;
using NameSiloDnsUpdateService.NameSilo.ApiModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NameSiloDnsUpdateService.NameSilo
{
    public class NameSiloRepository
    {
        private static XmlSerializer ApiResponseXmlSerializer => new XmlSerializer(typeof(ApiResponse));

        private readonly HttpClient httpClient;
        private readonly ApiConfiguration configuration;
        private readonly ILogger logger;

        public NameSiloRepository(IHttpClientFactory httpClientFactory, ApiConfiguration configuration, ILogger logger)
        {
            this.httpClient = httpClientFactory.CreateClient();
            this.configuration = configuration;
            this.logger = logger.ForContext<NameSiloRepository>();
        }

        public async Task<DnsRecordList> GetDnsRecordList()
        {
            var uri = new UriBuilder(configuration.BaseUrl)
            {
                Path = "api/dnsListRecords",
                Query = GenerateDefaultQueryStrings().ToDelimitedString("&")
            }.Uri;

            logger.Debug("Uri to NameSilo: {uri}", uri);

            using (var response = await httpClient.GetAsync(uri))
            {
                response.EnsureSuccessStatusCode();

                using (var content = response.Content)
                using (var xmlStream = await content.ReadAsStreamAsync())
                {
                    var apiResponse = (ApiResponse)ApiResponseXmlSerializer.Deserialize(xmlStream);
                    apiResponse.EnsureSuccessfulResponseCode();

                    return new DnsRecordList()
                    {
                        CallingIpAddress = IPAddress.Parse(apiResponse.Request.IPAddress),
                        ResourceRecords = apiResponse.Reply.ResourceRecords.Select(record =>
                        new DnsRecord
                        {
                            RecordID = record.RecordID,
                            Host = Regex.Replace(record.Host, $@"\.?{Regex.Escape(configuration.Domain)}$", ""),
                            Value = record.Value,
                            Type = record.Type,
                        }).ToArray(),
                    };
                }
            }
        }

        public async Task<string> UpdateHostIpAddress(UpdateHostMessage update)
        {
            var uri = new UriBuilder(configuration.BaseUrl)
            {
                Path = "api/dnsUpdateRecord",
                Query = GenerateDefaultQueryStrings().Concat(
                        new QueryStringParam[]
                        {
                            ( "rrid", update.RecordID ),
                            ( "rrhost", update.Host),
                            ( "rrvalue", update.IPAddress.MapToIPv4().ToString())
                        }
                    ).ToDelimitedString("&")
            }.Uri;

            using (var response = await httpClient.GetAsync(uri))
            {
                response.EnsureSuccessStatusCode();

                using (var content = response.Content)
                using (var xmlStream = await content.ReadAsStreamAsync())
                {
                    var apiResponse = (ApiResponse)ApiResponseXmlSerializer.Deserialize(xmlStream);

                    apiResponse.EnsureSuccessfulResponseCode();

                    return apiResponse.Reply.RecordID;
                }
            }
        }

        private IEnumerable<QueryStringParam> GenerateDefaultQueryStrings() =>
            new QueryStringParam[]
            {
                ( "key", configuration.ApiKey ),
                ( "domain", configuration.Domain ),
                ( "type", configuration.Type ),
                ( "version", configuration.Version ),
            };

        private class QueryStringParam
        {
            public string Key { get; set; }
            public string Value { get; set; }

            public override string ToString() => ToQueryString();

            public string ToQueryString() => $"{WebUtility.UrlEncode(Key)}={WebUtility.UrlEncode(Value)}";

            public static implicit operator QueryStringParam((string key, string value) t) =>
                new QueryStringParam { Key = t.key, Value = t.value };
        }
    }
}
