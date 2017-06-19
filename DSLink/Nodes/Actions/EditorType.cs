using System.Collections.Generic;

namespace DSLink.Nodes.Actions
{
    public class EditorType
    {
        public static readonly EditorType Color = new EditorType("color");

        public static readonly Dictionary<string, EditorType> Types = new Dictionary<string, EditorType>
        {
            {Color.Type, Color}
        };

        public readonly string Type;
        public readonly Value TypeValue;

        public EditorType(string editorType)
        {
            Type = editorType;
            TypeValue = new Value(Type);
        }

        public static EditorType FromString(string type)
        {
            type = type.ToLower();
            return Types.ContainsKey(type) ? Types[type] : null;
        }
    }
}
