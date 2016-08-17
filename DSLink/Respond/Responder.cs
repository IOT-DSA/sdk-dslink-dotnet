using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSLink.Connection.Serializer;
using System.IO;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Request;
using DSLink.Util.Logger;
using Newtonsoft.Json.Linq;
using PCLStorage;

namespace DSLink.Respond
{
    /// <summary>
    /// Class that handles the responder features.
    /// </summary>
    public sealed class Responder
    {
        /// <summary>
        /// DSLink container
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// Super root node
        /// </summary>
        public Node SuperRoot { get; }

        /// <summary>
        /// Subscription manager
        /// </summary>
        internal SubscriptionManager SubscriptionManager;

        /// <summary>
        /// Stream manager
        /// </summary>
        internal StreamManager StreamManager;

        /// <summary>
        /// Dictionary of Node Classes.
        /// </summary>
        internal IDictionary<string, Action<Node>> NodeClasses;

        /// <summary>
        /// Responder constructor
        /// </summary>
        /// <param name="link"></param>
        internal Responder(AbstractContainer link)
        {
            _link = link;
            SuperRoot = new Node("", null, _link);
            SubscriptionManager = new SubscriptionManager(_link);
            StreamManager = new StreamManager(_link);
            NodeClasses = new Dictionary<string, Action<Node>>();
        }

        /// <summary>
        /// Serialize the node structure.
        /// </summary>
        public async Task Serialize()
        {
            JObject obj = SuperRoot.Serialize();

            IFolder folder = await Configuration.GetStorageFolder();
            IFile file;

            try
            {
                file = await folder.GetFileAsync("nodes.json");
            }
            catch
            {
                file = await folder.CreateFileAsync("nodes.json", CreationCollisionOption.ReplaceExisting);
            }

            if (file != null)
            {
                string data = obj.ToString();
                await file.WriteAllTextAsync(data);
                string path = file.Path;
                if (_link.Config.LogLevel.DoesPrint(LogLevel.Debug))
                {
                    _link.Logger.Debug($"Wrote {data} to {path}");
                }
            }
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

        /// <summary>
        /// Deserialize the node structure.
        /// </summary>
        public async Task<bool> Deserialize()
        {
            IFolder folder = await Configuration.GetStorageFolder();

            try
            {
                IFile file = await folder.GetFileAsync("nodes.json");

                if (file != null)
                {
                    var data = await file.ReadAllTextAsync();

                    if (data != null)
                    {
                        JObject obj = JObject.Parse(data);
                        SuperRoot.Deserialize(obj);
                        return true;
                    }
                }
            }
            catch
            {
                _link.Logger.Debug("Failed to load the nodes.json");
            }
            return false;
        }

        /// <summary>
        /// Process a list of requests.
        /// </summary>
        /// <param name="requests">List of requests</param>
        /// <returns>List of responses</returns>
        internal async Task<JArray> ProcessRequests(JArray requests)
        {
            var responses = new JArray();
            foreach (JObject request in requests)
            {
                switch (request["method"].Value<string>())
                {
                    case "list":
                        {
                            var node = SuperRoot.Get(request["path"].Value<string>());
                            if (node != null)
                            {
                                StreamManager.Open(request["rid"].Value<int>(), node);
                                responses.Add(new JObject
                                {
                                    new JProperty("rid", request["rid"].Value<int>()),
                                    new JProperty("stream", "open"),
                                    new JProperty("updates", SuperRoot.Get(request["path"].Value<string>()).SerializeUpdates())
                                });
                            }
                        }
                        break;
                    case "set":
                        {
                            var node = SuperRoot.Get(request["path"].Value<string>());
                            if (node != null)
                            {
                                if (request["permit"] == null || request["permit"].Value<string>().Equals(node.GetConfig("writable").String))
                                {
                                    node.Value.Set(request["value"]);
                                    responses.Add(new JObject
                                    {
                                        new JProperty("rid", request["rid"].Value<int>()),
                                        new JProperty("stream", "closed")
                                    });
                                }
                            }
                        }
                        break;
                    case "remove":
                        {
                            SuperRoot.RemoveConfigAttribute(request["path"].Value<string>());
                            responses.Add(new JObject
                            {
                                new JProperty("rid", request["rid"].Value<int>()),
                                new JProperty("stream", "closed")
                            });
                        }
                        break;
                    case "invoke":
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
                        break;
                    case "subscribe":
                        {
                            foreach (var pair in request["paths"].Value<JArray>())
                            {
                                var pathToken = pair["path"];
                                var sidToken = pair["sid"];
                                if (pathToken != null && sidToken != null &&
                                    pair["path"].Type == JTokenType.String &&
                                    pair["sid"].Type == JTokenType.Integer)
                                {
                                    var node = SuperRoot.Get(pathToken.Value<string>());
                                    if (node != null)
                                    {
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
                                }
                            }
                            responses.Add(new JObject
                            {
                                new JProperty("rid", request["rid"].Value<int>()),
                                new JProperty("stream", "closed")
                            });
                        }
                        break;
                    case "unsubscribe":
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
                        break;
                    case "close":
                        {
                            if (request["rid"] != null)
                            {
                                StreamManager.Close(request["rid"].Value<int>());
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException(string.Format("Method {0} not implemented", request["method"].Value<string>()));
                }
            }
            return responses;
        }
    }

    /// <summary>
    /// Class to manage DSA subscriptions.
    /// </summary>
    internal class SubscriptionManager
    {
        /// <summary>
        /// Dictionary that maps a subscription ID to a Node.
        /// </summary>
        private readonly Dictionary<int, Node> _subscriptions;

        /// <summary>
        /// DSLink container instance.
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Respond.SubscriptionManager"/> class.
        /// </summary>
        /// <param name="link">Link container instance</param>
        public SubscriptionManager(AbstractContainer link)
        {
            _subscriptions = new Dictionary<int, Node>();
            _link = link;
        }

        /// <summary>
        /// Add a subscription to a Node with a subscription ID.
        /// </summary>
        /// <param name="sid">Subscription ID</param>
        /// <param name="node">Node to subscribe</param>
        public void Subscribe(int sid, Node node)
        {
            node.Subscribers.Add(sid);
            node.OnSubscribed?.Invoke(sid);
            _subscriptions.Add(sid, node);
        }

        /// <summary>
        /// Remove a subscription to a Node.
        /// </summary>
        /// <param name="sid">Subscription ID</param>
        public void Unsubscribe(int sid)
        {
            try
            {
                Node node = _subscriptions[sid];
                lock (node.Subscribers)
                {
                    _subscriptions[sid].Subscribers.Remove(sid);
                }
                _subscriptions[sid].OnUnsubscribed?.Invoke(sid);
                _subscriptions.Remove(sid);
            }
            catch (KeyNotFoundException)
            {
                _link.Logger.Debug($"Failed to Unsubscribe: unknown subscription id {sid}");
            }
        }

        /// <summary>
        /// Ran when the connection is lost.
        /// Clears all subscriptions.
        /// </summary>
        public void ClearAll()
        {
            _subscriptions.Clear();
        }
    }

    /// <summary>
    /// Class to manage DSA streams
    /// </summary>
    internal class StreamManager
    {
        /// <summary>
        /// Map of request IDs to a Node.
        /// </summary>
        private readonly Dictionary<int, Node> _streams = new Dictionary<int, Node>();

        /// <summary>
        /// DSLink container instance.
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// StreamManager constructor.
        /// </summary>
        /// <param name="link"></param>
        public StreamManager(AbstractContainer link)
        {
            _link = link;
        }

        /// <summary>
        /// Open a stream to a Node with a request ID.
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="node">Node for stream</param>
        public void Open(int requestId, Node node)
        {
            _streams.Add(requestId, node);
            lock (node.Streams)
            {
                node.Streams.Add(requestId);
            }
        }

        /// <summary>
        /// Close a stream to a Node with a request ID.
        /// </summary>
        /// <param name="requestId">Request ID</param>
        public void Close(int requestId)
        {
            try
            {
                lock (_streams[requestId].Streams)
                {
                    _streams[requestId].Streams.Remove(requestId);
                }
                _streams.Remove(requestId);
            }
            catch (KeyNotFoundException)
            {
                _link.Logger.Debug($"Failed to Close: unknown request id {requestId}");
            }
        }
                 
        /// <summary>
        /// Ran when the connection is lost.
        /// Clears all streams.
        /// </summary>
        internal void ClearAll()
        {
            _streams.Clear();
        }
    }
}
