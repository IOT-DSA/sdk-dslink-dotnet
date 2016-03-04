using System;
using System.Collections.Generic;

namespace DSLink.Connection.Serializer
{
    internal class SerializationManager
    {
        public static readonly Dictionary<string, Type> Serializers = new Dictionary<string, Type>()
        {
            //{"msgpack", typeof(MsgPackSerializer)},
            {"json", typeof(JsonSerializer)}
        };

        public ISerializer Serializer { get; }

        public SerializationManager(string serializerName)
        {
            Serializer = (ISerializer) Activator.CreateInstance(Serializers[serializerName]);
        }
    }
}
