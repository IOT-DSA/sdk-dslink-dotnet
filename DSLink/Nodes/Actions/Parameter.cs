using System.Collections.Generic;
using System.Diagnostics;
using DSLink.Connection.Serializer;
using Newtonsoft.Json;

namespace DSLink.Nodes.Actions
{
    public class Parameter : Serializable
    {
        private string _name;
        private string _type;
        private dynamic _default;

        [JsonProperty("name")]
        public string Name => _name;
        [JsonProperty("type")]
        public string Type => _type;
        [JsonProperty("default")]
        public string Default => _default;

        public Parameter(string name, string type, dynamic def = null)
        {
            this._name = name;
            this._type = type;
            _default = def;
        }

        public override Dictionary<dynamic, dynamic> Serialize()
        {
            var dict = new Dictionary<dynamic, dynamic>();
            dict["name"] = _name;
            dict["type"] = _type;
            dict["default"] = _default;
            return dict;
        }

        public override void Deserialize(Dictionary<dynamic, dynamic> data)
        {
            if (data.ContainsKey("name")) _name = data["name"];
            if (data.ContainsKey("type")) _type = data["type"];
            if (data.ContainsKey("default")) _default = data["default"];
        }
    }
}
