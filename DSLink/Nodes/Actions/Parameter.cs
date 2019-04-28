namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Parameter allows data to be passed into an action during an invoke.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Description of the parameter, shown in DGLux as
        /// a tooltip of the title.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Placeholder for editor field, only shown in DGLux
        /// when editor is equal to "textinput".
        /// </summary>
        public string Placeholder
        {
            get;
            set;
        }

        /// <summary>
        /// Type of the parameter.
        /// </summary>
        public ValueType ValueType
        {
            get;
            set;
        }

        /// <summary>
        /// Default value of the parameter.
        /// </summary>
        public Value DefaultValue
        {
            get;
            set;
        }

        /// <summary>
        /// Editor config of the parameter.
        /// </summary>
        public EditorType Editor
        {
            get;
            set;
        }

        public Parameter()
        {
        }

        public Parameter(string name, ValueType type, dynamic defaultValue = null, EditorType editor = null)
        {
            Name = name;
            ValueType = type;

            if (defaultValue != null)
            {
                DefaultValue = defaultValue;
            }

            if (editor != null)
            {
                Editor = editor;
            }
        }
    }
}