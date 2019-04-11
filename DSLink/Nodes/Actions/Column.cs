using Newtonsoft.Json.Linq;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Used to return data from an action while invoking.
    /// </summary>
    public class Column : JObject
    {
        /// <summary>
        /// Name of column.
        /// </summary>
        public string Name
        {
            get { return this["name"].Value<string>(); }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Type of the value attached to this column.
        /// </summary>
        public ValueType ValueType
        {
            get { return ValueType.FromString(this["type"].Value<string>()); }
            set { this["type"] = value.Type; }
        }

        public Column()
        {
        }

        public Column(string name, ValueType type)
        {
            Name = name;
            ValueType = type;
        }
    }
}