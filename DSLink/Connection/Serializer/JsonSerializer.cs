using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DSLink.Connection.Serializer
{
    public class JsonSerializer : ISerializer
    {
        public dynamic Serialize(RootObject data)
        {
            return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
				Converters = new List<JsonConverter>(){new JsonByteArrayConverter()}
            });
        }

        public RootObject Deserialize(dynamic data)
        {
            return JsonConvert.DeserializeObject<RootObject>(data);
        }
    }
}
