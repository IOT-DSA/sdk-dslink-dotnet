using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSLink.Serializer
{
    /// <summary>
    /// JSON implementation of serializer.
    /// </summary>
    public class JsonSerializer : BaseSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public JsonSerializer()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new JsonByteArrayConverter()
                }
            };
        }

        public override dynamic Serialize(JObject data)
        {
            return JsonConvert.SerializeObject(data, Formatting.None, _serializerSettings);
        }

        public override JObject Deserialize(dynamic data)
        {
            return JsonConvert.DeserializeObject(data, _serializerSettings);
        }
    }
}