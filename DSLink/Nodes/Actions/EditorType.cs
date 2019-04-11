using System.Collections.Generic;

namespace DSLink.Nodes.Actions
{
    public class EditorType
    {
        public static readonly EditorType Color = new EditorType("color");
        public static readonly EditorType Date = new EditorType("date");
        public static readonly EditorType TextArea = new EditorType("textarea");
        public static readonly EditorType Password = new EditorType("password");
        public static readonly EditorType DateRange = new EditorType("daterange");

        public static readonly Dictionary<string, EditorType> Types = new Dictionary<string, EditorType>
        {
            {Color.Type, Color},
            {Date.Type, Date},
            {TextArea.Type, TextArea},
            {Password.Type, Password},
            {DateRange.Type, DateRange}
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