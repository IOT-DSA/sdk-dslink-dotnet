using Newtonsoft.Json.Linq;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Parameter for an action.
    /// </summary>
    public class Parameter : JObject
    {
        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string Name
        {
            get
            {
                return this["name"].Value<string>();
            }
        }

        public string ValueType
        {
            get
            {
                return this["type"].Value<string>();
            }
        }

        public string Default
        {
            get
            {
                return this["default"].Value<string>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Nodes.Actions.Parameter"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        /// <param name="def">Default</param>
        public Parameter(string name, string type, dynamic def = null)
        {
            this["name"] = name;
            this["type"] = type;
            this["default"] = def;
        }
    }
}
