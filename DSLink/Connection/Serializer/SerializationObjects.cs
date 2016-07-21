using System.Collections.Generic;
using DSLink.Nodes.Actions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSLink.Connection.Serializer
{
    /// <summary>
    /// Root DSA object.
    /// </summary>
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

    /// <summary>
    /// Request object.
    /// </summary>
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
        public JToken Value;
        // TODO: Swap out dynamic for Parameter
        [JsonProperty("params")]
        public Dictionary<string, JToken> Parameters;
    }

    /// <summary>
    /// Response object.
    /// </summary>
    public class ResponseObject
    {
        [JsonProperty("rid")]
        public int? RequestId;
        [JsonProperty("stream")]
        public string Stream;
        [JsonProperty("meta")]
        public Dictionary<string, dynamic> Meta;
        [JsonProperty("columns")]
        public List<Column> Columns;
        [JsonProperty("updates")]
        public JArray Updates;
    }

    /// <summary>
    /// Add subscription object.
    /// </summary>
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
