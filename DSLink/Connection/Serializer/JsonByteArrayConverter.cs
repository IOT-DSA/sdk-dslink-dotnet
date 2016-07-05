using System;
using Newtonsoft.Json;
using DSLink.Util;
using JSONSerializer = Newtonsoft.Json.JsonSerializer;

namespace DSLink.Connection.Serializer
{
	public class JsonByteArrayConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(byte[]);
		}

		public override void WriteJson(JsonWriter writer, object value, JSONSerializer serializer)
		{
			writer.WriteValue("\x1B" + "bytes:" + UrlBase64.Encode((byte[]) value));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JSONSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
