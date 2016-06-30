using System.Collections.Generic;
using System.Diagnostics;
using DSLink.Connection.Serializer;

namespace DSLink.Nodes.Actions
{
    public class Parameter : Serializable
    {
        public string name;
        public string type;
        public dynamic @default;

        public Parameter(string name, string type, dynamic def = null)
        {
            this.name = name;
            this.type = type;
            @default = def;
        }

        public override Dictionary<dynamic, dynamic> Serialize()
        {
            var dict = new Dictionary<dynamic, dynamic>();
            dict["name"] = name;
            dict["type"] = type;
            dict["default"] = @default;
            return dict;
        }

        public override void Deserialize(Dictionary<dynamic, dynamic> data)
        {
            if (data.ContainsKey("name")) name = data["name"];
            if (data.ContainsKey("type")) type = data["type"];
            if (data.ContainsKey("default")) @default = data["default"];
        }
    }
}
