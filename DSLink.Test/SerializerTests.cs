using DSLink.Serializer;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;

namespace DSLink.Test
{
    [TestFixture]
    public class SerializerTests
    {
        private static readonly JsonSerializer _json = new JsonSerializer();
        private static readonly MsgPackSerializer _msgpack = new MsgPackSerializer();

        private static readonly JObject _simpleMsgAck = new JObject
        {
            new JProperty("msg", 123),
            new JProperty("ack", 62)
        };
        private static readonly string _simpleMsgAck_Json_Expected = "{\"msg\":123,\"ack\":62}";
        private static readonly byte[] _simpleMsgAck_MsgPack_Expected =
        {
            0x82, 0xA3, 0x6D, 0x73, 0x67, 0x7B, 0xA3, 0x61, 0x63, 0x6B, 0x3E
        };

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
        private static readonly byte[] _listRequest_MsgPack_Expected =
        {
            0x83, 0xA3, 0x6D, 0x73, 0x67, 0x7B, 0xA3, 0x61, 0x63, 0x6B,
            0x3E, 0xA8, 0x72, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x73,
            0x91, 0x83, 0xA3, 0x72, 0x69, 0x64, 0x01, 0xA6, 0x6D, 0x65,
            0x74, 0x68, 0x6F, 0x64, 0xA4, 0x6C, 0x69, 0x73, 0x74, 0xA4,
            0x70, 0x61, 0x74, 0x68, 0xA5, 0x2F, 0x74, 0x65, 0x73, 0x74
        };

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
        private static readonly byte[] _binaryUpdate_MsgPack_Expected =
        {
            0x83, 0xA3, 0x6D, 0x73, 0x67, 0x7B, 0xA3, 0x61, 0x63, 0x6B,
            0x3E, 0xA9, 0x72, 0x65, 0x73, 0x70, 0x6F, 0x6E, 0x73, 0x65,
            0x73, 0x91, 0x82, 0xA3, 0x72, 0x69, 0x64, 0x00, 0xA7, 0x75,
            0x70, 0x64, 0x61, 0x74, 0x65, 0x73, 0x91, 0x93, 0x00, 0xC4,
            0x06, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0xBA, 0x32, 0x30,
            0x31, 0x34, 0x2D, 0x31, 0x31, 0x2D, 0x32, 0x37, 0x54, 0x30,
            0x39, 0x3A, 0x31, 0x31, 0x2E, 0x30, 0x30, 0x30, 0x2D, 0x30,
            0x38, 0x3A, 0x30, 0x30
        };

        private void Json_Common(JObject jObj, string expected, bool deepEquals = true)
        {
            var serialized = _json.Serialize(jObj);
            Assert.AreEqual(serialized.GetType(), typeof(string));
            Assert.AreEqual(serialized, expected);
            var deserialized = _json.Deserialize(serialized);
            if (deepEquals)
                Assert.IsTrue(JObject.DeepEquals(jObj, deserialized));
        }

        private void MsgPack_Common(JObject jObj, byte[] expected)
        {
            var serialized = _msgpack.Serialize(jObj);
            Console.WriteLine(BitConverter.ToString(serialized));
            Assert.AreEqual(serialized.GetType(), typeof(byte[]));
            Assert.AreEqual(serialized, expected);
            var deserialized = _msgpack.Deserialize(serialized);
            Assert.IsTrue(JObject.DeepEquals(jObj, deserialized));
        }

        [Test]
        public void Json_SimpleMsgAck()
        {
            Json_Common(_simpleMsgAck, _simpleMsgAck_Json_Expected);
        }

        [Test]
        public void Json_ListRequest()
        {
            Json_Common(_listRequest, _listRequest_Json_Expected);
        }

        [Test]
        public void Json_BinaryUpdate()
        {
            // JsonSerializer does not output proper binary, Value takes care of that.
            // Skip DeepEquals
            // TODO: Enable DeepEquals when JsonSerializer supports binary deserialization.
            Json_Common(_binaryUpdate, _binaryUpdate_Json_Expected, false);
        }

        [Test]
        public void MsgPack_SimpleMsgAck()
        {
            MsgPack_Common(_simpleMsgAck, _simpleMsgAck_MsgPack_Expected);
        }

        [Test]
        public void MsgPack_ListRequest()
        {
            MsgPack_Common(_listRequest, _listRequest_MsgPack_Expected);
        }

        [Test]
        public void MsgPack_BinaryUpdate()
        {
            MsgPack_Common(_binaryUpdate, _binaryUpdate_MsgPack_Expected);
        }
    }
}
