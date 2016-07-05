using System;
using System.Collections.Generic;
using DSLink.Connection.Serializer;

namespace Benchmarks.NET
{
    public class Serialization
    {
        private JsonSerializer _json;
        private MsgPackSerializer _msgpack;
        private RootObject _serializeObject;

        public Serialization()
        {
            _json = new JsonSerializer();
            _msgpack = new MsgPackSerializer();
            var random = new Random();
            var byteBuffer = new byte[50000000];
            random.NextBytes(byteBuffer);
            _serializeObject = new RootObject
            {
                Responses = new List<ResponseObject>
                {
                    new ResponseObject
                    {
                        Updates = new List<dynamic>
                        {
                            new List<dynamic>
                            {
                                byteBuffer
                            }
                        }
                    }
                }
            };
        }

        public void JsonSerialize()
        {
            _json.Serialize(_serializeObject);
        }

        public void MsgPackSerialize()
        {
            _msgpack.Serialize(_serializeObject);
        }
    }
}

