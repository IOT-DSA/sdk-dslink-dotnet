using Newtonsoft.Json.Linq;

namespace DSLink.Connection.Serializer
{
    /// <summary>
    /// Interface for serialization
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// True if the connection used requires binary stream support.
        /// </summary>
        bool RequiresBinaryStream
        {
            get;
        }

        /// <summary>
        /// Serialize the specified data.
        /// </summary>
        /// <param name="data">Data to serialize in serialization object form.</param>
        /// <returns>Serialized data</returns>
        dynamic Serialize(JObject data);

        /// <summary>
        /// Deserialize the specified data.
        /// </summary>
        /// <param name="data">Data in serialized form.</param>
        /// <returns>Deserialized data in serialization object form.</returns>
        JObject Deserialize(dynamic data);
    }
}
