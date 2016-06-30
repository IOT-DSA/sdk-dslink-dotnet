using System.IO;
using DSLink.MsgPack;

namespace DSLink.Connection.Serializer
{
    public class MsgPackSerializer : ISerializer
    {
        public dynamic Serialize(RootObject data)
        {
            var encoder = new MsgPackEncoder();
            encoder.Pack(data.Serialize());
            return encoder.ToArray();
        }

        public RootObject Deserialize(dynamic data)
        {
            var stream = new MemoryStream(data);
            var decoder = new MsgPackDecoder(stream);
            var dict = decoder.Unpack();
            var root = new RootObject();
            root.Deserialize(dict);
            return root;
        }
    }
}
