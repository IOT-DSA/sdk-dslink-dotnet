using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics;
using DSLink.Nodes.Actions;

namespace DSLink.MsgPack
{
    public class MsgPackEncoder
    {
        private MemoryStream _stream;
        private BinaryWriter _writer;

        public MsgPackEncoder()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        public void Pack(dynamic obj)
        {
            if (obj == null)
            {
                Write(0xc0);
            }
            else if (obj is bool && obj == false)
            {
                Write(0xc2);
            }
            else if (obj is bool && obj == true)
            {
                Write(0xc3);
            }
            else if (obj is sbyte)
            {
                PackInt8(obj);
            }
            else if (obj is short)
            {
                PackInt16(obj);
            }
            else if (obj is int)
            {
                PackInt32(obj);
            }
            else if (obj is long)
            {
                PackInt64(obj);
            }
            else if (obj is byte)
            {
                PackUInt8(obj);
            }
            else if (obj is ushort)
            {
                PackUInt16(obj);
            }
            else if (obj is uint)
            {
                PackUInt32(obj);
            }
            else if (obj is ulong)
            {
                PackUInt64(obj);
            }
            else if (obj is float)
            {
                PackFloat(obj);
            }
            else if (obj is double)
            {
                PackDouble(obj);
            }
            else if (obj is IDictionary)
            {
                PackDictionary(obj);
            }
            else if (obj is byte[])
            {
                PackBinary(obj);
            }
            else if (obj is IList)
            {
                PackList(obj);
            }
            else if (obj is string)
            {
                PackString(obj);
            }
            else if (obj is Parameter)
            {
                PackDictionary(((Parameter)obj).Serialize());
            }
            else if (obj is Column)
            {
                PackDictionary(((Column)obj).Serialize());
            }
            else
            {
                throw new Exception(string.Format("Could not pack type {0}.", obj));
            }
        }

        private void Write(dynamic data)
        {
            _writer.Write((byte)data);
        }

        private void WriteBytes(byte[] bytes)
        {
            foreach (byte bite in bytes)
            {
                Write(bite);
            }
        }

        public void PackInt8(sbyte value)
        {
            Write(0xd0);
            _writer.Write(value);
        }

        public void PackInt16(short value)
        {
            Write(0xd1);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            WriteBytes(bytes);
        }

        public void PackInt32(int value)
        {
            Write(0xd2);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            WriteBytes(bytes);
        }

        public void PackInt64(long value)
        {
            Write(0xd3);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            WriteBytes(bytes);
        }

        public void PackUInt8(byte value)
        {
            Write(0xcc);
            Write(value);
        }

        public void PackUInt16(ushort value)
        {
            Write(0xcd);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            WriteBytes(bytes);
        }

        public void PackUInt32(uint value)
        {
            Write(0xce);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            WriteBytes(bytes);
        }

        public void PackUInt64(ulong value)
        {
            Write(0xcf);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            WriteBytes(bytes);
        }

        public void PackFloat(float value)
        {
            Write(0xca);
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            WriteBytes(data);
        }

        public void PackDouble(double value)
        {
            Write(0xcb);
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            WriteBytes(data);
        }

        public void PackString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            if (bytes.Length < 0x20)
            {
                Write(0xa0 + bytes.Length);
            }
            else if (bytes.Length < 0x100)
            {
                Write(0xd9);
                Write(bytes.Length);
            }
            else if (bytes.Length < 0x10000)
            {
                Write(0xda);
                byte[] bites = BitConverter.GetBytes(bytes.Length);
                Array.Reverse(bites);
                WriteBytes(bites);
            }
            else
            {
                Write(0xdb);
                byte[] bites = BitConverter.GetBytes(bytes.Length);
                Array.Reverse(bites);
                WriteBytes(bites);

            }
            WriteBytes(bytes);
        }

        public void PackList<TValue>(IList<TValue> values)
        {
            if (values.Count < 16)
            {
                Write(0x90 + values.Count);
            }
            else if (values.Count < 0x100)
            {
                Write(0xdc);
                byte[] bytes = BitConverter.GetBytes(values.Count);
                Array.Reverse(bytes);
                WriteBytes(bytes);
            }
            else
            {
                Write(0xdd);
                byte[] bytes = BitConverter.GetBytes(values.Count);
                Array.Reverse(bytes);
                WriteBytes(bytes);
            }

            foreach (TValue value in values)
            {
                Pack(value);
            }
        }

        public void PackDictionary<TKey, TValue>(IDictionary<TKey, TValue> values)
        {
            if (values.Count < 16)
            {
                Write(0x80 + values.Count);
            }
            else if (values.Count < 0x100)
            {
                Write(0xde);
                byte[] bytes = BitConverter.GetBytes(values.Count);
                Array.Reverse(bytes);
                WriteBytes(bytes);
            }
            else
            {
                Write(0xdf);
                byte[] bytes = BitConverter.GetBytes(values.Count);
                Array.Reverse(bytes);
                WriteBytes(bytes);

            }

            foreach (KeyValuePair<TKey, TValue> value in values)
            {
                Pack(value.Key);
                Pack(value.Value);
            }
        }

        public void PackBinary(byte[] data)
        {
            if (data.Length <= 255)
            {
                Write(0xc4);
                Write(data.Length);
            }
            else if (data.Length <= 65535)
            {
                Write(0xc5);
                Write((data.Length >> 8) & 0xff);
                Write(data.Length & 0xff);
            }
            else
            {
                Write(0xc6);
                Write((data.Length >> 24) & 0xff);
                Write((data.Length >> 16) & 0xff);
                Write((data.Length >> 8) & 0xff);
                Write(data.Length & 0xff);
            }

            WriteBytes(data);
        }
    }

    public class MsgPackDecoder
    {
        private BinaryReader _reader;
        private const uint UINT32_MAX = 4294967295;

        public MsgPackDecoder(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public dynamic Unpack()
        {
            byte type = _reader.ReadByte();

            if (type >= 0xe0) return type - 0x100;
            if (type < 0xc0)
            {
                if (type < 0x80) return type;
                else if (type < 0x90) return UnpackDictionary<dynamic, dynamic>((uint)type - 0x80);
                else if (type < 0xa0) return UnpackList<dynamic>((uint)type - 0x90);
                else return UnpackString((uint)type - 0xa0);
            }

            switch (type)
            {
                case 0xc0:
                    return null;
                case 0xc2:
                    return false;
                case 0xc3:
                    return true;

                case 0xc4:
                    return UnpackBinary(type);
                case 0xc5:
                    return UnpackBinary(type);
                case 0xc6:
                    return UnpackBinary(type);

                case 0xcc:
                    return UnpackUInt8();
                case 0xcd:
                    return UnpackUInt16();
                case 0xce:
                    return UnpackUInt32();
                case 0xcf:
                    return UnpackUInt64();

                case 0xd0:
                    return UnpackInt8();
                case 0xd1:
                    return UnpackInt16();
                case 0xd2:
                    return UnpackInt32();
                case 0xd3:
                    return UnpackInt64();

                case 0xd9:
                    return UnpackString(UnpackUInt8());
                case 0xda:
                    return UnpackString(UnpackUInt16());
                case 0xdb:
                    return UnpackString(UnpackUInt32());

                case 0x80:
                    return UnpackDictionary<dynamic, dynamic>(UnpackUInt8());
                case 0xde:
                    return UnpackDictionary<dynamic, dynamic>(UnpackUInt16());
                case 0xdf:
                    return UnpackDictionary<dynamic, dynamic>(UnpackUInt32());

                case 0x90:
                    return UnpackList<dynamic>(UnpackUInt8());
                case 0xdc:
                    return UnpackList<dynamic>(UnpackUInt16());
                case 0xdd:
                    return UnpackList<dynamic>(UnpackUInt32());

                case 0xca:
                    return UnpackFloat();
                case 0xcb:
                    return UnpackDouble();
            }
            throw new Exception("Could not unpack type.");
        }

        private byte[] ReadBytes(uint count)
        {
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = _reader.ReadByte();
            }
            return bytes;
        }

        public byte[] UnpackBinary(byte type)
        {
            uint length = 0;
            switch (type)
            {
                case 0xc4:
                    length = UnpackUInt8();
                    break;
                case 0xc5:
                    length = UnpackUInt16();
                    break;
                case 0xc6:
                    length = UnpackUInt32();
                    break;
            }

            return ReadBytes(length);
        }

        public float UnpackFloat()
        {
            byte[] bytes = ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        public double UnpackDouble()
        {
            byte[] bytes = ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        public byte UnpackUInt8()
        {
            return _reader.ReadByte();
        }

        public ushort UnpackUInt16()
        {
            byte[] bytes = ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public uint UnpackUInt32()
        {
            byte[] bytes = ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public ulong UnpackUInt64()
        {
            byte[] bytes = ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public sbyte UnpackInt8()
        {
            return _reader.ReadSByte();
        }

        public short UnpackInt16()
        {
            byte[] bytes = ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public int UnpackInt32()
        {
            byte[] bytes = ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public long UnpackInt64()
        {
            byte[] bytes = ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public string UnpackString(uint length)
        {
            return Encoding.UTF8.GetString(ReadBytes(length), 0, (int) length);
        }

        public Dictionary<TKey, TValue> UnpackDictionary<TKey, TValue>(uint length)
        {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < length; ++i)
            {
                dict.Add(Unpack(), Unpack());
            }
            return dict;
        }

        public List<TValue> UnpackList<TValue>(uint length)
        {
            var list = new List<TValue>();
            for (int i = 0; i < length; i++)
            {
                list.Add(Unpack());
            }
            return list;
        }
    }
}
