using DSLink.Serializer;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;

namespace DSLink.Tests
{
    [TestFixture]
    public class SerializerTests
    {
        private static readonly JsonSerializer _json = new JsonSerializer(null);
        private static readonly MsgPackSerializer _msgpack = new MsgPackSerializer(null);

        private static readonly JObject _simpleMsgAck = new JObject
        {
            new JProperty("msg", 123),
            new JProperty("ack", 62)
        };
        private static readonly string _simpleMsgAck_Json_Expected = "{\"msg\":123,\"ack\":62}";

        private static readonly JObject _listRequest = new JObject
        {
            new JProperty("msg", 123),
            new JProperty("ack", 62),
            new JProperty("requests", new JArray
            {
                new JObject
                {
                    new JProperty("rid", 1),
                    new JProperty("method", "list"),
                    new JProperty("path", "/test")
                }
            })
        };
        private static readonly string _listRequest_Json_Expected = "{\"msg\":123,\"ack\":62,\"requests\":[{\"rid\":1,\"method\":\"list\",\"path\":\"/test\"}]}";

        private static readonly JObject _binaryUpdate = new JObject
        {
            new JProperty("msg", 123),
            new JProperty("ack", 62),
            new JProperty("responses", new JArray
            {
                new JObject
                {
                    new JProperty("rid", 0),
                    new JProperty("updates", new JArray
                    {
                        new JArray
                        {
                            0,
                            new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 },
                            "2014-11-27T09:11.000-08:00"
                        }
                    })
                }
            })
        };
        private static readonly string _binaryUpdate_Json_Expected = "{\"msg\":123,\"ack\":62,\"responses\":[{\"rid\":0,\"updates\":[[0,\"\\u001bbytes:AAECAwQF\",\"2014-11-27T09:11.000-08:00\"]]}]}";

        private void TestJson_Common(JObject jobj, string expected)
        {
            var serialized = _json.Serialize(jobj);
            Console.WriteLine(serialized);
            Assert.AreEqual(serialized.GetType(), typeof(string));
            Assert.AreEqual(serialized, expected);
            var deserialized = _json.Deserialize(serialized);
            Assert.IsTrue(JObject.DeepEquals(jobj, deserialized));
        }

        [Test]
        public void TestJson_SimpleMsgAck()
        {
            TestJson_Common(_simpleMsgAck, _simpleMsgAck_Json_Expected);
        }

        [Test]
        public void TestJson_ListRequest()
        {
            TestJson_Common(_listRequest, _listRequest_Json_Expected);
        }

        [Test]
        public void TestJson_BinaryUpdate()
        {
            //TestJson_Common(_binaryUpdate, _binaryUpdate_Json_Expected);
        }
    }
}
