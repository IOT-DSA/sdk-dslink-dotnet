using Newtonsoft.Json;

namespace DSLink.Nodes.Actions
{
    public class Column
    {
        [JsonProperty("name")]
        public readonly string Name;

        [JsonProperty("type")]
        public readonly string Type;

        public Column(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
