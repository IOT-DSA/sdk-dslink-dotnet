using System;
using System.Collections.Generic;
using MsgPack;
using MsgPack.Serialization;

namespace DSLink.Connection.Serializer
{
    /*public class MsgPackSerializer : ISerializer
    {
        private readonly MessagePackSerializer<Dictionary<string, dynamic>> _serializer = SerializationContext.Default.GetSerializer<Dictionary<string, dynamic>>();

        public dynamic Serialize(Dictionary<string, dynamic> data)
        {
            return _serializer.PackSingleObject(data);
        }

        public Dictionary<string, dynamic> Deserialize(dynamic data)
        {
            return FromMsgPack(_serializer.UnpackSingleObject(data));
        }

        public dynamic FromMsgPack(dynamic data)
        {
            if (data is Dictionary<string, object>)
            {
                Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();
                foreach (var pair in (Dictionary<string, object>) data)
                {
                    //dict.Add(pair.Key.AsString(), FromMsgPack(pair.Value));
                }
            }
            if (data is MessagePackObject)
            {
                MessagePackObject mpo = data;
                bool isString = mpo.IsTypeOf(typeof (string)).Value;
                bool isBool = mpo.IsTypeOf(typeof (bool)).Value;
                if (isString)
                {
                    return mpo.AsString();
                }
                if (isBool)
                {
                    return mpo.AsBoolean();
                }
            }
            throw new Exception("Unhandled type " + data.ToString());
        }
    }*/
}
