using DSLink.Container;
using Newtonsoft.Json.Linq;

namespace DSLink.Serializer
{
    /// <summary>
    /// Interface for serialization
    /// </summary>
    public abstract class BaseSerializer
    {
        protected AbstractContainer _link;

        /// <summary>
        /// True if the connection used requires binary stream support.
        /// </summary>
        bool RequiresBinaryStream
        {
            get;
        }

        public BaseSerializer(AbstractContainer link)
        {
            _link = link;
        }

        /// <summary>
        /// Serialize the specified data.
        /// </summary>
        /// <param name="data">Data to serialize in serialization object form.</param>
        /// <returns>Serialized data</returns>
        public abstract dynamic Serialize(JObject data);

        /// <summary>
        /// Deserialize the specified data.
        /// </summary>
        /// <param name="data">Data in serialized form.</param>
        /// <returns>Deserialized data in serialization object form.</returns>
        public abstract JObject Deserialize(dynamic data);
    }
}
