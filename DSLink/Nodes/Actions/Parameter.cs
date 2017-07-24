using DSLink.Util;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Used to pass data to an action while invoking.
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
                return this["default"].ToDynamic();
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
        
        public Parameter()
        {}

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
