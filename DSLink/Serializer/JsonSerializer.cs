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
        public bool RequiresBinaryStream => false;

        private readonly JsonByteArrayConverter _byteArrayConverter;
        private readonly JsonSerializerSettings _serializerSettings;

        public JsonSerializer(DSLinkContainer link) : base(link)
        {
            _byteArrayConverter = new JsonByteArrayConverter();
            _serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    _byteArrayConverter
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
