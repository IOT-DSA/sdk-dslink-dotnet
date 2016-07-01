using System;
using System.Collections.Generic;
using DSLink.MsgPack;
using Newtonsoft.Json;

namespace MsgPackTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var encoder = new MsgPackEncoder();
            //encoder.Pack((ushort)18);
            encoder.Pack(new Dictionary<string, dynamic>
            {
                {"msg", 1},
                {"ack", 1},
                {
                    "responses",
                    new List<dynamic>
                    {
                        new Dictionary<string, dynamic>
                        {
                            {"rid", 1},
                            {"stream", "open"},
                            {"updates", new List<dynamic>()}
                        },
                        0,
                        18
                    }
                }
            });
            Console.WriteLine(BitConverter.ToString(encoder.ToArray()));
            encoder.stream.Position = 0;
            var decoder = new MsgPackDecoder(encoder.stream);
            var dict = decoder.Unpack();
            var s = JsonConvert.SerializeObject(dict);
            Console.WriteLine(s);
            //Console.WriteLine(BitConverter.ToString(encoder.ToArray()));
        }
    }
}
