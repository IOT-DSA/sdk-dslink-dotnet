using System;
using System.Threading.Tasks;
using DSLink.Nodes;
using DSLink.Request;
using Newtonsoft.Json.Linq;

namespace DSLink.Respond
{
    public class DSLinkResponder : Responder
    {
        public DSLinkResponder(DSLinkContainer link) : base()
        {
            Link = link;
        }

        public override void Init()
        {
            DiskSerializer = new DiskSerializer(this);
            SubscriptionManager = new SubscriptionManager(Link);
            StreamManager = new StreamManager(Link);
            SuperRoot = new Node("", null, Link);
        }

        public override void AddNodeClass(string name, Action<Node> factory)
        {
            lock (NodeClasses)
            {
                NodeClasses[name] = factory;
            }
        }

        public override async Task<JArray> ProcessRequests(JArray requests)
        {
            var responses = new JArray();
            foreach (var jToken in requests)
            {
                var request = (JObject) jToken;
                switch (request["method"].Value<string>())
                {
                    case "list":
                        ListMethod(responses, request);
                        break;
                    case "set":
                        SetMethod(responses, request);
                        break;
                    case "remove":
                        RemoveMethod(responses, request);
                        break;
                    case "invoke":
                        await InvokeMethod(request);
                        break;
                    case "subscribe":
                        SubscribeMethod(responses, request);
                        break;
                    case "unsubscribe":
                        UnsubscribeMethod(responses, request);
                        break;
                    case "close":
                        CloseMethod(request);
                        break;
                    default:
                        throw new ArgumentException($"Method {request["method"].Value<string>()} not implemented");
                }
            }
            return responses;
        }

        private void ListMethod(JArray responses, JObject request)
        {
            var node = SuperRoot.Get(request["path"].Value<string>());
            if (node != null)
            {
                StreamManager.OpenStream(request["rid"].Value<int>(), node);
                responses.Add(new JObject
                {
                    new JProperty("rid", request["rid"].Value<int>()),
                    new JProperty("stream", "open"),
                    new JProperty("updates", SuperRoot.Get(request["path"].Value<string>()).SerializeUpdates())
                });
            }
            else
            {
                StreamManager.OpenStreamLater(request["rid"].Value<int>(), request["path"].Value<string>());
            }
        }

        private void SetMethod(JArray responses, JObject request)
        {
            var node = SuperRoot.Get(request["path"].Value<string>());
            if (node != null)
            {
                if (request["permit"] == null ||
                    request["permit"].Value<string>().Equals(node.Configs.Get(ConfigType.Writable).String))
                {
                    node.Value.Set(request["value"]);
                    node.Value.InvokeRemoteSet();
                    responses.Add(new JObject
                    {
                        new JProperty("rid", request["rid"].Value<int>()),
                        new JProperty("stream", "closed")
                    });
                }
            }
        }

        private void RemoveMethod(JArray responses, JObject request)
        {
            SuperRoot.RemoveConfigAttribute(request["path"].Value<string>());
            responses.Add(new JObject
            {
                new JProperty("rid", request["rid"].Value<int>()),
                new JProperty("stream", "closed")
            });
        }

        private async Task InvokeMethod(JObject request)
        {
            var node = SuperRoot.Get(request["path"].Value<string>());
            if (node?.ActionHandler != null)
            {
                if (request["permit"] == null || request["permit"].Value<string>().Equals(node.ActionHandler.Permission.ToString()))
                {
                    JArray columns;
                    if (node.Configs.Has(ConfigType.Columns))
                    {
                        columns = node.Configs.Get(ConfigType.Columns).JArray;
                    }
                    else
                    {
                        columns = new JArray();
                    }
                    var permit = (request["permit"] != null) ? Permission.PermissionMap[request["permit"].Value<string>().ToLower()] : null;
                    var invokeRequest = new InvokeRequest(request["rid"].Value<int>(), request["path"].Value<string>(),
                                                          permit, request["params"].Value<JObject>(), link: Link,
                                                          columns: columns);
                    await Task.Run(() => node.ActionHandler.Function.Invoke(invokeRequest));
                }
            }
        }

        private void SubscribeMethod(JArray responses, JObject request)
        {
            foreach (var pair in request["paths"].Value<JArray>())
            {
                var pathToken = pair["path"];
                var sidToken = pair["sid"];
                if (pathToken == null || sidToken == null || pathToken.Type != JTokenType.String ||
                    sidToken.Type != JTokenType.Integer) continue;
                var node = SuperRoot.Get(pathToken.Value<string>());
                if (node == null) continue;
                var sid = sidToken.Value<int>();
                SubscriptionManager.Subscribe(sid, node);
                responses.Add(new JObject
                {
                    new JProperty("rid", 0),
                    new JProperty("updates", new JArray
                    {
                        new JArray
                        {
                            sid,
                            node.Value.JToken,
                            node.Value.LastUpdated
                        }
                    })
                });
            }
            responses.Add(new JObject
            {
                new JProperty("rid", request["rid"].Value<int>()),
                new JProperty("stream", "closed")
            });
        }

        private void UnsubscribeMethod(JArray responses, JObject request)
        {
            foreach (var sid in request["sids"].Value<JArray>())
            {
                SubscriptionManager.Unsubscribe(sid.Value<int>());
            }
            responses.Add(new JObject
            {
                new JProperty("rid", request["rid"].Value<int>()),
                new JProperty("stream", "closed")
            });
        }

        private void CloseMethod(JObject request)
        {
            if (request["rid"] != null)
            {
                StreamManager.CloseStream(request["rid"].Value<int>());
            }
        }
    }
}
