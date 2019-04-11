using System.Collections.Generic;

namespace DSLink.Nodes
{
    public class ResultType
    {
        public static readonly ResultType Values = new ResultType("values");
        public static readonly ResultType Stream = new ResultType("stream");
        public static readonly ResultType Table = new ResultType("table");

        public readonly string Name;
        public readonly Value Value;

        public ResultType(string name)
        {
            Name = name;
            Value = new Value(Name);
        }
    }
}