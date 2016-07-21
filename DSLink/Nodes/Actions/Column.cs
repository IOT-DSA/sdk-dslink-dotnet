using Newtonsoft.Json.Linq;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Column of an action.
    /// </summary>
    public class Column : JObject
    {
        /// <summary>
        /// Name of the column.
        /// </summary>
        public string Name
        {
            get
            {
                return this["name"].Value<string>();
            }
        }

        /// <summary>
        /// Type of the column.
        /// </summary>
        public string ValueType
        {
            get
            {
                return this["type"].Value<string>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Nodes.Actions.Column"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        public Column(string name, string type)
        {
            this["name"] = name;
            this["type"] = type;
        }
    }
}
