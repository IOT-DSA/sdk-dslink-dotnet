using System;
using Newtonsoft.Json;
using DSLink.Util;
using JSONSerializer = Newtonsoft.Json.JsonSerializer;

namespace DSLink.Serializer
{
    /// <summary>
    /// Custom serializer extension for JSON, which serializes binary data to
    /// a standard put forth by DSA.
    /// <see cref="JsonSerializer"/> 
    /// </summary>
	public class JsonByteArrayConverter : JsonConverter
	{
        /// <summary>
        /// Whether the converter is applicable for this type.
        /// </summary>
        /// <param name="objectType">Type</param>
        /// <returns>True if this converter is applicable</returns>
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(byte[]);
		}

        /// <summary>
        /// Generate the JSON data.
        /// </summary>
        /// <param name="writer">Stream to write to</param>
        /// <param name="value">Value</param>
        /// <param name="serializer">Serializer instance</param>
		public override void WriteJson(JsonWriter writer, object value, JSONSerializer serializer)
		{
			writer.WriteValue("\x1B" + "bytes:" + UrlBase64.Encode((byte[]) value));
		}

        /// <summary>
        /// Not implemented. Required by interface. Actual deserialization
        /// happens in Value.
        /// <see cref="Nodes.Value"/> 
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JSONSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
