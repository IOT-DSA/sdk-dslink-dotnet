using System;
using System.Collections.Generic;

namespace DSLink.Connection.Serializer
{
    /// <summary>
    /// Manages serializers.
    /// </summary>
    internal class SerializationManager
    {
        /// <summary>
        /// Dictionary of serializers.
        /// </summary>
        public static readonly Dictionary<string, Type> Serializers = new Dictionary<string, Type>()
        {
            {"msgpack", typeof(MsgPackSerializer)},
            {"json", typeof(JsonSerializer)}
        };

        /// <summary>
        /// Selected serializer.
        /// </summary>
        /// <value>The serializer.</value>
        public ISerializer Serializer { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Connection.Serializer.SerializationManager"/> class.
        /// </summary>
        /// <param name="serializerName">Serializer name.</param>
        public SerializationManager(string serializerName)
        {
            Serializer = (ISerializer) Activator.CreateInstance(Serializers[serializerName]);
        }
    }
}
