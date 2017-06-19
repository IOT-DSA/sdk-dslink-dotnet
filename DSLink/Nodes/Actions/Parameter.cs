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

        /// <summary>
        /// Type of the parameter.
        /// </summary>
        public ValueType ValueType
        {
            get
            {
                return ValueType.FromString(this["type"].Value<string>());
            }
            set
            {
                this["type"] = value.Type;
            }
        }

        /// <summary>
        /// Default value of the parameter.
        /// </summary>
        public dynamic DefaultValue
        {
            get
            {
                return this["default"].Value<dynamic>();
            }
            set
            {
                this["default"] = value;
            }
        }

        /// <summary>
        /// Editor config of the parameter.
        /// </summary>
        public EditorType Editor
        {
            get
            {
                return EditorType.FromString(this["editor"].Value<string>());
            }
            set
            {
                this["editor"] = value.Type;
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
        public Parameter(string name, ValueType type, dynamic defaultValue = null, EditorType editor = null)
        {
            Name = name;
            ValueType = type;
            if (defaultValue != null)
                DefaultValue = defaultValue;
            if (editor != null)
                Editor = editor;
        }
    }
}
