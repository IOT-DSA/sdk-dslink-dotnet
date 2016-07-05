using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSLink.Connection.Serializer
{
    /// <summary>
    /// JSON implementation of serializer.
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        /// <summary>
        /// <see cref="ISerializer.RequiresBinaryStream"/>
        /// </summary>
        public bool RequiresBinaryStream => false;

        /// <summary>
        /// <see cref="ISerializer.Serialize(RootObject)"/> 
        /// </summary>
        public dynamic Serialize(RootObject data)
        {
            return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
				Converters = new List<JsonConverter>(){new JsonByteArrayConverter()}
            });
        }

        /// <summary>
        /// Deserialize the specified data.
        /// </summary>
        /// <param name="data">Data.</param>
        public RootObject Deserialize(dynamic data)
        {
            return JsonConvert.DeserializeObject<RootObject>(data);
        }
    }
}
