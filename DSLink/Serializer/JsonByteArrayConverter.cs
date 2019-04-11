using System;
using Newtonsoft.Json;
using DSLink.Util;
using JSONSerializer = Newtonsoft.Json.JsonSerializer;

namespace DSLink.Serializer
{
    /// <inheritdoc />
    /// <summary>
    /// Custom serializer extension for JSON, which serializes binary data to
    /// a standard put forth by DSA.
    /// <see cref="T:DSLink.Serializer.JsonSerializer" /> 
    /// </summary>
    public class JsonByteArrayConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]) || objectType == typeof(string);
        }

        public override void WriteJson(JsonWriter writer, object value, JSONSerializer serializer)
        {
            var type = value.GetType();
            if (type == typeof(byte[]))
            {
                writer.WriteValue("\x1B" + "bytes:" + UrlBase64.Encode((byte[]) value));
            }
            else if (type == typeof(string))
            {
                writer.WriteValue((string) value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JSONSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                return null;
            }

            var val = (string) reader.Value;
            if (val.StartsWith("\x1B" + "bytes:") || val.StartsWith("\\u001bbytes:"))
            {
                return UrlBase64.Decode(val.Substring(val.IndexOf(":", StringComparison.Ordinal) + 1));
            }

            return val;
        }
    }
}