using Newtonsoft.Json;

namespace DSLink.Connection.Serializer
{
    public class JsonSerializer : ISerializer
    {
        public dynamic Serialize(RootObject data)
        {
            return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public RootObject Deserialize(dynamic data)
        {
            return JsonConvert.DeserializeObject<RootObject>(data);
        }
    }
}
