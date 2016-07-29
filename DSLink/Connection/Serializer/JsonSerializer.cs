using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        /// <see cref="ISerializer.Serialize(JObject)"/> 
        /// </summary>
        public dynamic Serialize(JObject data)
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

        /// <summary>
        /// Deserialize the specified data.
        /// </summary>
        /// <param name="data">Data</param>
        public JObject Deserialize(dynamic data)
        {
            return JsonConvert.DeserializeObject(data);
        }
    }
}
