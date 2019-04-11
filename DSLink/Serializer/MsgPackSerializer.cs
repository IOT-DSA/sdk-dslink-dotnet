using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Msgpack;
using JSONSerializer = Newtonsoft.Json.JsonSerializer;

namespace DSLink.Serializer
{
    /// <summary>
    /// MessagePack implementation of serializer. Uses an extension of Json.NET,
    /// which uses its backend and generates MessagePack data via MsgPack.Cli.
    /// </summary>
    public class MsgPackSerializer : BaseSerializer
    {
        private readonly JSONSerializer _serializer;

        public MsgPackSerializer()
        {
            _serializer = new JSONSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public override dynamic Serialize(JObject data)
        {
            var stream = new MemoryStream();
            var writer = new MessagePackWriter(stream);
            _serializer.Serialize(writer, data);
            return stream.ToArray();
        }

        public override JObject Deserialize(dynamic data)
        {
            var stream = new MemoryStream(data);
            var reader = new MessagePackReader(stream);
            var jObj = _serializer.Deserialize<JObject>(reader);
            return jObj;
        }
    }
}