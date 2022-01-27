namespace NameSiloDynDns

open NameSiloDynDns.NameSilo
open NameSiloDynDns.NameSilo.ApiModels
open Serilog
open System
open System.Net
open System.Net.Http
open System.Text.RegularExpressions
open System.Xml.Serialization

type ApiConfiguration =
    { BaseUrl: string
      Domain: string
      ApiKey: string
      Version: string
      Type: string }

type NameSiloHttpClient(httpClient: HttpClient) =
    member val HttpClient = httpClient

type NameSiloRepository(nameSiloHttpClient: NameSiloHttpClient, configuration: ApiConfiguration, logger: ILogger) =
    let ApiResponseXmlSerializer = XmlSerializer(typeof<ApiResponse>)
    let httpClient = nameSiloHttpClient.HttpClient
    let logger = logger.ForContext<NameSiloRepository>()

    let createQueryString pairs =
        pairs
        |> List.map (fun (key, value) -> $"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}")
        |> String.concat "&"

    let defaultQueryStrings =
        [ "key", configuration.ApiKey
          "domain", configuration.Domain
          "type", configuration.Type
          "version", configuration.Version ]

    member this.GetDnsRecordList() =
        task {
            let uri =
                UriBuilder(
                    configuration.BaseUrl,
                    Path = "api/dnsListRecords",
                    Query = createQueryString defaultQueryStrings
                )
                    .Uri

            logger.Debug("Uri to NameSilo: {uri}", uri)

            use! response = httpClient.GetAsync(uri)
            response.EnsureSuccessStatusCode() |> ignore

            use content = response.Content
            use! xmlStream = content.ReadAsStreamAsync()

            let apiResponse =
                ApiResponseXmlSerializer.Deserialize(xmlStream) :?> ApiResponse

            apiResponse.EnsureSuccessfulResponseCode()

            return
                { CallingIpAddress = IPAddress.Parse(apiResponse.Request.IPAddress)
                  ResourceRecords =
                    apiResponse.Reply.ResourceRecords
                    |> Array.map (fun record ->
                        { RecordID = record.RecordID
                          Host = Regex.Replace(record.Host, $@"\.?{Regex.Escape(configuration.Domain)}$", "")
                          Value = record.Value
                          Type = record.Type }) }
        }

    member this.UpdateHostIpAddress(update: UpdateHostMessage) =
        task {
            let uri =
                UriBuilder(
                    configuration.BaseUrl,
                    Path = "api/dnsUpdateRecord",
                    Query =
                        createQueryString [ yield! defaultQueryStrings
                                            "rrid", update.RecordID
                                            "rrhost", update.Host
                                            "rrvalue", update.IPAddress.MapToIPv4().ToString() ]
                )
                    .Uri

            use! response = httpClient.GetAsync(uri)
            response.EnsureSuccessStatusCode() |> ignore

            use content = response.Content
            use! xmlStream = content.ReadAsStreamAsync()

            let apiResponse =
                ApiResponseXmlSerializer.Deserialize(xmlStream) :?> ApiResponse

            apiResponse.EnsureSuccessfulResponseCode()
            return apiResponse.Reply.RecordID
        }
