using System.Collections.Generic;
using DSLink.Nodes.Actions;
using Newtonsoft.Json;

namespace DSLink.Connection.Serializer
{
    public class RootObject
    {
        [JsonProperty("salt")]
        public string Salt;
        [JsonProperty("msg")]
        public int? Msg;
        [JsonProperty("ack")]
        public int? Ack;
        [JsonProperty("requests")]
        public List<RequestObject> Requests;
        [JsonProperty("responses")]
        public List<ResponseObject> Responses;
    }

    public class RequestObject
    {
        [JsonProperty("rid")]
        public int? RequestId;
        [JsonProperty("method")]
        public string Method;
        [JsonProperty("path")]
        public string Path;
        [JsonProperty("paths")]
        public List<AddSubscriptionObject> Paths;
        [JsonProperty("sids")]
        public List<int> SubscriptionIds; 
        [JsonProperty("permit")]
        public string Permit;
        [JsonProperty("value")]
        public dynamic Value;
        // TODO: Swap out dynamic for Parameter
        [JsonProperty("params")]
        public Dictionary<string, Parameter> Parameters;
    }

    public class ResponseObject
    {
        [JsonProperty("rid")]
        public int? RequestId;
        [JsonProperty("stream")]
        public string Stream;
        [JsonProperty("updates")]
        public List<dynamic> Updates;
        [JsonProperty("columns")]
        public List<dynamic> Columns;
    }

    public class AddSubscriptionObject
    {
        [JsonProperty("path")]
        public string Path;
        [JsonProperty("sid")]
        public int? SubscriptionId;
        [JsonProperty("qos")]
        public int? QualityOfService;
    }
}
