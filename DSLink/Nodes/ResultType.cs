using System.Collections.Generic;

namespace DSLink.Nodes
{
    public class ResultType
    {
        public static readonly ResultType Values = new ResultType("values");
        public static readonly ResultType Stream = new ResultType("stream");
        public static readonly ResultType Table = new ResultType("table");

        public static readonly Dictionary<string, ResultType> ResultTypes = new Dictionary<string, ResultType>
        {
            {Values.Name, Values},
            {Stream.Name, Stream},
            {Table.Name, Table}
        };

        public readonly string Name;
        public readonly Value Value;

        public ResultType(string name)
        {
            Name = name;
            Value = new Value(Name);
        }

        public static ResultType FromString(string resultType)
        {
            resultType = resultType.ToLower();
            return ResultTypes.ContainsKey(resultType) ? ResultTypes[resultType] : null;
        }
    }
}
