using Newtonsoft.Json;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Column of an action.
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Name of the column.
        /// </summary>
        [JsonProperty("name")]
        public readonly string Name;

        /// <summary>
        /// Type of the column.
        /// </summary>
        [JsonProperty("type")]
        public readonly string Type;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Nodes.Actions.Column"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        public Column(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
