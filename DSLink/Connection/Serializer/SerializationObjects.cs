using System.Collections.Generic;
using System.Linq;
using DSLink.Nodes.Actions;

namespace DSLink.Connection.Serializer
{
    public class RootObject : Serializable
    {
        public string salt;
        public int? msg;
        public int? ack;
        public List<RequestObject> requests;
        public List<ResponseObject> responses;

        public override Dictionary<dynamic, dynamic> Serialize()
        {
            var dict = new Dictionary<dynamic, dynamic>();
            if (salt != null) dict["salt"] = salt;
            if (msg.HasValue) dict["msg"] = msg.Value;
            if (ack.HasValue) dict["ack"] = ack.Value;
            if (requests != null && requests.Count > 0)
            {
                dict["requests"] = new List<dynamic>();
                foreach (RequestObject request in requests)
                {
                    dict["requests"].Add(request.Serialize());
                }
            }
            if (responses != null && responses.Count > 0)
            {
                dict["responses"] = new List<dynamic>();
                foreach (ResponseObject response in responses)
                {
                    dict["responses"].Add(response.Serialize());
                }
            }
            return dict;
        }

        public override void Deserialize(Dictionary<dynamic, dynamic> data)
        {
            if (data.ContainsKey("salt")) salt = data["salt"];
            if (data.ContainsKey("msg")) msg = (int)data["msg"];
            if (data.ContainsKey("ack")) ack = (int)data["ack"];
            if (data.ContainsKey("requests"))
            {
                requests = new List<RequestObject>();
                foreach (dynamic dict in data["requests"])
                {
                    var req = new RequestObject();
                    req.Deserialize(dict);
                    requests.Add(req);
                }
            }
            if (data.ContainsKey("responses"))
            {
                responses = new List<ResponseObject>();
                foreach (Dictionary<dynamic, dynamic> dict in data["responses"])
                {
                    var resp = new ResponseObject();
                    resp.Deserialize(dict);
                    responses.Add(resp);
                }
            }
        }
    }

    public class RequestObject : Serializable
    {
        public int? rid;
        public string method;
        public string path;
        public List<AddSubscriptionObject> paths;
        public List<int> sids;
        public string permit;
        public dynamic value;
        // TODO: Swap out dynamic for Parameter
        public Dictionary<string, dynamic> @params;

        public override Dictionary<dynamic, dynamic> Serialize()
        {
            var dict = new Dictionary<dynamic, dynamic>();
            if (rid.HasValue) dict["rid"] = rid;
            if (method != null) dict["method"] = method;
            if (path != null) dict["path"] = path;
            if (paths != null && paths.Count > 0)
            {
                dict["paths"] = new List<dynamic>();
                foreach (AddSubscriptionObject addSub in paths)
                {
                    dict["paths"].Add(addSub.Serialize());
                }
            }
            if (sids != null && sids.Count > 0) dict["sids"] = sids;
            if (permit != null) dict["permit"] = permit;
            if (@params != null) dict["params"] = @params;
            /*{
                dict["params"] = new Dictionary<string, Parameter>();
                foreach(KeyValuePair<string, dynamic> kp in @params)
                {
                    dict["params"].Add(kp.Key, kp.Value.Serialize());
                }
            }*/
            return dict;
        }

        public override void Deserialize(Dictionary<dynamic, dynamic> data)
        {
            if (data.ContainsKey("rid")) rid = data["rid"];
            if (data.ContainsKey("method")) method = data["method"];
            if (data.ContainsKey("path")) path = data["path"];
            if (data.ContainsKey("paths"))
            {
                paths = new List<AddSubscriptionObject>();
                foreach (var i in data["paths"])
                {
                    var subObj = new AddSubscriptionObject();
                    subObj.Deserialize(i);
                    paths.Add(subObj);
                }
            }
            if (data.ContainsKey("sids"))
            {
                sids = ((List<object>)data["sids"]).OfType<int>().ToList();
            }
            if (data.ContainsKey("permit")) permit = data["permit"];
            if (data.ContainsKey("value")) value = data["value"];
            if (data.ContainsKey("params"))
            {
                @params = new Dictionary<string, dynamic>();
                foreach (KeyValuePair<dynamic, dynamic> kp in data["params"])
                {
                    @params.Add(kp.Key, kp.Value);
                }
            }
            /*{
                @params = new Dictionary<string, Parameter>();
                foreach (KeyValuePair<dynamic, dynamic> kp in data["params"])
                {
                    var parameter = new Parameter(null, null);
                    parameter.Deserialize(kp.Value);
                    @params.Add(kp.Key, parameter);
                }
            }*/
        }
    }

    public class ResponseObject : Serializable
    {
        public int? rid;
        public string stream;
        public Dictionary<string, dynamic> meta;
        public List<Column> columns;
        public List<dynamic> updates;

        public override Dictionary<dynamic, dynamic> Serialize()
        {
            var dict = new Dictionary<dynamic, dynamic>();
            if (rid.HasValue) dict["rid"] = rid.Value;
            if (stream != null) dict["stream"] = stream;
            if (meta != null && meta.Count > 0) dict["meta"] = meta;
            if (columns != null && columns.Count > 0)
            {
                dict["columns"] = new List<dynamic>();
                foreach (Column column in columns)
                {
                    dict["columns"].Add(column.Serialize());
                }
            }
            if (updates != null && updates.Count > 0) dict["updates"] = updates;
            return dict;
        }

        public override void Deserialize(Dictionary<dynamic, dynamic> data)
        {
            if (data.ContainsKey("rid")) rid = data["rid"];
            if (data.ContainsKey("stream")) stream = data["stream"];
            if (data.ContainsKey("meta")) meta = data["meta"];
            if (data.ContainsKey("columns"))
            {
                columns = new List<Column>();
                foreach (var col in data["columns"])
                {
                    var column = new Column(null, null);
                    column.Deserialize(col);
                    columns.Add(column);
                }
            }
            if (data.ContainsKey("updates")) updates = data["updates"];
        }
    }

    public class AddSubscriptionObject : Serializable
    {
        public string path;
        public int? sid;
        public int? qos;

        public override Dictionary<dynamic, dynamic> Serialize()
        {
            var dict = new Dictionary<dynamic, dynamic>();
            if (path != null) dict["path"] = path;
            if (sid.HasValue) dict["sid"] = sid;
            if (qos.HasValue) dict["qos"] = qos;
            return dict;
        }

        public override void Deserialize(Dictionary<dynamic, dynamic> data)
        {
            if (data.ContainsKey("path")) path = data["path"];
            if (data.ContainsKey("sid")) sid = data["sid"];
            if (data.ContainsKey("qos")) qos = data["qos"];
        }
    }
}
