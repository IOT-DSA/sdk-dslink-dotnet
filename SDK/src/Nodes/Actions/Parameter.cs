using Newtonsoft.Json;

namespace DSLink.Nodes.Actions
{
    public class Parameter
    {
        [JsonProperty("name")]
        public readonly string Name;
        [JsonProperty("type")]
        public readonly string Type;
        [JsonProperty("default")]
        public readonly dynamic Default;

        public Parameter(string name, string type, dynamic def = null)
        {
            Name = name;
            Type = type;
            Default = def;
        }
    }
}
