using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSLink.Container;

namespace DSLink.Connection.Serializer
{
    /// <summary>
    /// JSON implementation of serializer.
    /// </summary>
    public class JsonSerializer : BaseSerializer
    {
        public bool RequiresBinaryStream => false;

        public JsonSerializer(AbstractContainer link) : base(link)
        {}

        public override dynamic Serialize(JObject data)
        {
            return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new JsonByteArrayConverter()
                }
            });
        }

        public override JObject Deserialize(dynamic data)
        {
            return JsonConvert.DeserializeObject(data);
        }
    }
}
