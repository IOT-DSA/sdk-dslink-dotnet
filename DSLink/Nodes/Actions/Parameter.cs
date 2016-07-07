using Newtonsoft.Json;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Parameter for an action.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Name of the parameter.
        /// </summary>
        [JsonProperty("name")]
        public readonly string Name;

        /// <summary>
        /// Type of the parameter.
        /// </summary>
        [JsonProperty("type")]
        public readonly string Type;

        /// <summary>
        /// Default value of the parameter.
        /// </summary>
        [JsonProperty("default")]
        public readonly dynamic Default;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Nodes.Actions.Parameter"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        /// <param name="def">Default</param>
        public Parameter(string name, string type, dynamic def = null)
        {
            Name = name;
            Type = type;
            Default = def;
        }
    }
}
