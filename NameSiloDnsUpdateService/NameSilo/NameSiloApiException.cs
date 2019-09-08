using NameSiloDnsUpdateService.NameSilo.ApiModels;
using System;
using System.Runtime.Serialization;

namespace NameSiloDnsUpdateService.NameSilo
{
    public class NameSiloApiException : Exception
    {
        public string Operation { get; set; }
        public string Code { get; }

        public NameSiloApiException(ApiResponse apiResponse) : this(apiResponse.Reply.Detail)
        {
            Operation = apiResponse.Request.Operation;
            Code = apiResponse.Reply.Code;
        }

        public NameSiloApiException()
        {
        }

        public NameSiloApiException(string message) : base(message)
        {
        }

        public NameSiloApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NameSiloApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
