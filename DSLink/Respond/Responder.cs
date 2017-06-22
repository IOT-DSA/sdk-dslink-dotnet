using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Request;
using DSLink.Util.Logger;
using Newtonsoft.Json.Linq;
using PCLStorage;
using DSLink.Platform;

namespace DSLink.Respond
{
    public sealed class Responder
    {
        private readonly AbstractContainer _link;
        public readonly Node SuperRoot;
        internal SubscriptionManager SubscriptionManager;
        internal StreamManager StreamManager;
        internal IDictionary<string, Action<Node>> NodeClasses;

        public Responder(AbstractContainer link)
        {
            _link = link;
            SuperRoot = new Node("", null, _link);
            SubscriptionManager = new SubscriptionManager(_link);
            StreamManager = new StreamManager(_link);
            NodeClasses = new Dictionary<string, Action<Node>>();
        }

        /// <summary>
        /// Serialize and save the node structure to disk for
        /// loading when the DSLink starts again.
        /// </summary>
        public async Task SerializeToDisk()
        {
            JObject obj = SuperRoot.Serialize();
            IFolder folder = await BasePlatform.Current.GetStorageFolder();
            IFile file = await folder.CreateFileAsync("nodes.json", CreationCollisionOption.ReplaceExisting);

            if (file != null)
            {
                var data = obj.ToString();
                await file.WriteAllTextAsync(data);
                var path = file.Path;
                if (_link.Config.LogLevel.DoesPrint(LogLevel.Debug))
                {
                    _link.Logger.Debug($"Wrote {data} to {path}");
                }
            }
        }

        /// <summary>
        /// Deserializes nodes.json from the disk, and restores the node
        /// structure to the loaded data.
        /// </summary>
        /// <returns>True on success</returns>
        public async Task<bool> DeserializeFromDisk()
        {
            try
            {
                var folder = await BasePlatform.Current.GetStorageFolder();
                var file = await folder.GetFileAsync("nodes.json");

                if (file != null)
                {
                    var data = await file.ReadAllTextAsync();

                    if (data != null)
                    {
                        SuperRoot.Deserialize(JObject.Parse(data));
                        return true;
                    }
                }
            }
            catch
            {
                _link.Logger.Debug("Failed to load nodes.json");
            }

            return false;
        }

        /// <summary>
        /// Adds a new node class to the responder.
        /// </summary>
        /// <param name="name">Name of the class.</param>
        /// <param name="factory">Factory function for the class. First parameter is the node.</param>
        public void AddNodeClass(string name, Action<Node> factory)
        {
            lock (NodeClasses)
            {
                NodeClasses[name] = factory;
            }
        }

        internal async Task<JArray> ProcessRequests(JArray requests)
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
                if (request["permit"] == null || request["permit"].Value<string>().Equals(node.GetConfig("writable").String))
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
                    JArray columns = node.Columns ?? new JArray();
                    var permit = (request["permit"] != null) ? Permission.PermissionMap[request["permit"].Value<string>().ToLower()] : null;
                    var invokeRequest = new InvokeRequest(request["rid"].Value<int>(), request["path"].Value<string>(),
                                                          permit, request["params"].Value<JObject>(), link: _link,
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
                if (pathToken == null || sidToken == null || pair["path"].Type != JTokenType.String ||
                    pair["sid"].Type != JTokenType.Integer) continue;
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
                            pair["sid"].Value<int>(),
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

    internal class SubscriptionManager
    {
        private readonly Dictionary<int, Node> _subscriptionToNode;
        private readonly AbstractContainer _link;

        public SubscriptionManager(AbstractContainer link)
        {
            _subscriptionToNode = new Dictionary<int, Node>();
            _link = link;
        }

        public void Subscribe(int subscriptionId, Node node)
        {
            node.Subscribers.Add(subscriptionId);
            node.OnSubscribed?.Invoke(subscriptionId);
            _subscriptionToNode.Add(subscriptionId, node);
        }

        public void Unsubscribe(int sid)
        {
            try
            {
                var node = _subscriptionToNode[sid];
                lock (node.Subscribers)
                {
                    _subscriptionToNode[sid].Subscribers.Remove(sid);
                }
                _subscriptionToNode[sid].OnUnsubscribed?.Invoke(sid);
                _subscriptionToNode.Remove(sid);
            }
            catch (KeyNotFoundException)
            {
                _link.Logger.Debug($"Failed to Unsubscribe: unknown subscription id {sid}");
            }
        }

        public void ClearAll()
        {
            _subscriptionToNode.Clear();
        }
    }

    internal class StreamManager
    {
        private readonly Dictionary<int, string> _requestIdToPath = new Dictionary<int, string>();
        private readonly AbstractContainer _link;

        public StreamManager(AbstractContainer link)
        {
            _link = link;
        }

        public void OpenStream(int requestId, Node node)
        {
            _requestIdToPath.Add(requestId, node.Path);
            lock (node.Streams)
            {
                node.Streams.Add(requestId);
            }
        }

        public void OpenStreamLater(int requestId, string path)
        {
            _requestIdToPath.Add(requestId, path);
        }

        public void CloseStream(int requestId)
        {
            try
            {
                var node = _link.Responder.SuperRoot.Get(_requestIdToPath[requestId]);
                if (node != null)
                {
                    lock (node.Streams)
                    {
                        node.Streams.Remove(requestId);
                    }
                }
                _requestIdToPath.Remove(requestId);
            }
            catch (KeyNotFoundException)
            {
                _link.Logger.Debug($"Failed to Close: unknown request id or node for {requestId}");
            }
        }

        public void OnActivateNode(Node node)
        {
            foreach (var id in _requestIdToPath.Keys)
            {
                var path = _requestIdToPath[id];
                if (path == node.Path)
                {
                    node.Streams.Add(id);
                }
            }
        }
                 
        internal void ClearAll()
        {
            _requestIdToPath.Clear();
        }
    }
}
