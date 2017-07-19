using System;
using System.Collections.Generic;

namespace DSLink.Serializer
{
    internal static class Serializers
    {
        public static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>()
        {
            {"msgpack", typeof(MsgPackSerializer)},
            {"json", typeof(JsonSerializer)}
        };
    }
}
