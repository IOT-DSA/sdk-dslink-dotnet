using System;
using System.Collections.Generic;
using System.Text;

namespace DSLink.Nodes
{
    public class ValueType
    {
        public static readonly ValueType String = new ValueType("string");
        public static readonly ValueType Number = new ValueType("number");
        public static readonly ValueType Boolean = new ValueType("bool");
        public static readonly ValueType Map = new ValueType("map");
        public static readonly ValueType Array = new ValueType("array");
        public static readonly ValueType Dynamic = new ValueType("dynamic");
        public static readonly ValueType Binary = new ValueType("binary");

        public static readonly Dictionary<string, ValueType> Types = new Dictionary<string, ValueType>
        {
            {String.Type, String},
            {Number.Type, Number},
            {Boolean.Type, Boolean},
            {Map.Type, Map},
            {Array.Type, Array},
            {Dynamic.Type, Dynamic},
            {Binary.Type, Binary}
        };

        public readonly string Type;
        public readonly Value TypeValue;

        public ValueType(string valueType)
        {
            Type = valueType;
            TypeValue = new Value(Type);
        }

        public static ValueType FromString(string type)
        {
            type = type.ToLower();
            return Types.ContainsKey(type) ? Types[type] : null;
        }

        public static ValueType MakeEnum(params object[] values)
        {
            StringBuilder sb = new StringBuilder("enum[");
            foreach (object val in values)
            {
                sb.Append(val.ToString()).Append(',');
            }
            sb.Append(']');
            return new ValueType(sb.ToString());
        }
    }
}
