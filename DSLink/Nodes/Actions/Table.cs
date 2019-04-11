using Newtonsoft.Json.Linq;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Represents a DSA table, wraps JArray.
    /// </summary>
    public class Table : JArray
    {
        /// <summary>
        /// Represents DSA table modes.
        /// </summary>
        public class Mode
        {
            /// <summary>
            /// Refresh the table each update sent.
            /// </summary>
            public static readonly Mode Refresh = new Mode("refresh");

            /// <summary>
            /// Append to the table each update sent.
            /// </summary>
            public static readonly Mode Append = new Mode("append");

            /// <summary>
            /// Stream to the table.
            /// </summary>
            public static readonly Mode Stream = new Mode("stream");

            public readonly string String;

            public Mode(string modeString)
            {
                String = modeString;
            }
        }
    }
}