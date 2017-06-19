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
            set
            {
                this["name"] = value;
            }
        }

        public string ValueType
        {
            get
            {
                return this["type"].Value<string>();
            }
            set
            {
                this["type"] = value;
            }
        }

        public string DefaultValue
        {
            get
            {
                return this["default"].Value<string>();
            }
            set
            {
                this["default"] = value;
            }
        }

        public string Editor
        {
            get
            {
                return this["editor"].Value<string>();
            }
            set
            {
                this["editor"] = value;
            }
        }
        
        /// <summary>
        /// Empty constructor to allow for object initializers.
        /// </summary>
        public Parameter()
        {}

        /// <summary>
        /// Constructor that takes commonly used 
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="def">Default value</param>
        public Parameter(string name, string type, dynamic defaultValue = null, string editor = null)
        {
            Name = name;
            ValueType = type;
            if (defaultValue != null)
                DefaultValue = defaultValue;
            if (!string.IsNullOrEmpty(editor))
                Editor = editor;
        }
    }
}
