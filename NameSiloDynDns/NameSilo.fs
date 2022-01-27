module NameSiloDynDns.NameSilo

open System.Net

exception NameSiloApiException of detail: string * operation: string * code: string with
    override this.Message = this.detail

type DnsRecord =
    { RecordID: string
      Host: string
      Value: string
      Type: string }

type DnsRecordList =
    { CallingIpAddress: IPAddress
      ResourceRecords: DnsRecord array }

type UpdateHostMessage =
    { RecordID: string
      Host: string
      IPAddress: IPAddress }

module ApiModels =
    open System.Xml.Serialization

    type ResourceRecord =
        { [<XmlElement("record_id")>]
          RecordID: string
          [<XmlElement("type")>]
          Type: string
          [<XmlElement("host")>]
          Host: string
          [<XmlElement("value")>]
          Value: string
          [<XmlElement("ttl")>]
          TimeToLive: int
          [<XmlElement("distance")>]
          Distance: int }

    type Request =
        { [<XmlElement("operation")>]
          Operation: string
          [<XmlElement("ip")>]
          IPAddress: string }

    type Reply =
        { [<XmlElement("code")>]
          Code: string
          [<XmlElement("detail")>]
          Detail: string
          [<XmlElement("resource_record")>]
          ResourceRecords: ResourceRecord array
          [<XmlElement("record_id")>]
          RecordID: string }

    [<XmlRoot("namesilo")>]
    type ApiResponse =
        { [<XmlElement("request")>]
          Request: Request
          [<XmlElement("reply")>]
          Reply: Reply }
        member this.IsApiSuccessful = this.Reply.Code = "300"

        member this.EnsureSuccessfulResponseCode() =
            if not this.IsApiSuccessful then
                raise (NameSiloApiException(this.Reply.Detail, this.Request.Operation, this.Reply.Code))
