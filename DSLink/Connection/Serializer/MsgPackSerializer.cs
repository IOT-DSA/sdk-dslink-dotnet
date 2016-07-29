using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Msgpack;
using JSONSerializer = Newtonsoft.Json.JsonSerializer;

namespace DSLink.Connection.Serializer
{
    /// <summary>
    /// MessagePack implementation of serializer. Uses an extension of Json.NET,
    /// which uses its backend and generates MessagePack data via MsgPack.Cli.
    /// </summary>
    public class MsgPackSerializer : ISerializer
    {
        /// <summary>
        /// Json.NET Serializer backend.
        /// </summary>
        private readonly JSONSerializer _serializer;

        /// <summary>
        /// <see cref="ISerializer.RequiresBinaryStream"/> 
        /// </summary>
        public bool RequiresBinaryStream => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DSLink.Connection.Serializer.MsgPackSerializer"/> class.
        /// </summary>
        public MsgPackSerializer()
        {
            _serializer = new JSONSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        /// <summary>
        /// <see cref="ISerializer.Serialize(RootObject)"/>
        /// </summary>
        public dynamic Serialize(JObject data)
        {
            var stream = new MemoryStream();
            var writer = new MessagePackWriter(stream);
            _serializer.Serialize(writer, data);
            return stream.ToArray();
        }

        /// <summary>
        /// <see cref="ISerializer.Deserialize(dynamic)"/>
        /// </summary>
        public JObject Deserialize(dynamic data)
        {
            var stream = new MemoryStream(data);
            var reader = new MessagePackReader(stream);
            return _serializer.Deserialize<JObject>(reader);
        }
    }
}
