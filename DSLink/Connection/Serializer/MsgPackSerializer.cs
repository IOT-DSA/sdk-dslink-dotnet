using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Msgpack;
using JSONSerializer = Newtonsoft.Json.JsonSerializer;

namespace DSLink.Connection.Serializer
{
    public class MsgPackSerializer : ISerializer
    {
        private readonly JSONSerializer _serializer;

        public MsgPackSerializer()
        {
            _serializer = new JSONSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public dynamic Serialize(RootObject data)
        {
            var stream = new MemoryStream();
            var writer = new MessagePackWriter(stream);
            _serializer.Serialize(writer, data);
            return stream.ToArray();
        }

        public RootObject Deserialize(dynamic data)
        {
            var stream = new MemoryStream(data);
            var reader = new MessagePackReader(stream);
            return _serializer.Deserialize<RootObject>(reader);
        }
    }
}
