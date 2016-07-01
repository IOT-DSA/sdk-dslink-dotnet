using System;
using System.Collections.Generic;
using DSLink.Connection.Serializer;
using Newtonsoft.Json;

namespace DSLink.Nodes.Actions
{
    public class Column : Serializable
    {
        private string _name;
        private string _type;

        [JsonProperty("name")]
        public string Name => _name;
        [JsonProperty("type")]
        public string Type => _type;

        public Column(string name, string type)
        {
            this._name = name;
            this._type = type;
        }

        public override Dictionary<dynamic, dynamic> Serialize()
        {
            var dict = new Dictionary<dynamic, dynamic>();
            if (_name != null) dict["name"] = _name;
            if (_type != null) dict["type"] = _type;
            return dict;
        }

        public override void Deserialize(Dictionary<dynamic, dynamic> data)
        {
            if (data.ContainsKey("name")) _name = data["name"];
            if (data.ContainsKey("type")) _type = data["type"];
        }
    }
}
