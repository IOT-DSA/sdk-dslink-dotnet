using System;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes.Actions
{
    public class Table : JArray
    {
        public class Mode
        {
            public static readonly Mode Refresh = new Mode("refresh");
            public static readonly Mode Append = new Mode("append");
            public static readonly Mode Stream = new Mode("stream");

            public readonly string String;

            public Mode(string modeString)
            {
                String = modeString;
            }
        }
    }
}
