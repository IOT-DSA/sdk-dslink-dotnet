using System;
using System.Collections.Generic;
using System.Linq;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;

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
        /// Responder constructor
        /// </summary>
        /// <param name="link"></param>
        internal Responder(AbstractContainer link)
        {
            _link = link;
            SuperRoot = new Node("", null, _link);
            SubscriptionManager = new SubscriptionManager(_link);
            StreamManager = new StreamManager(_link);
        }

        /// <summary>
        /// Process a list of requests.
        /// </summary>
        /// <param name="requests">List of requests</param>
        /// <returns>List of responses</returns>
        internal List<ResponseObject> ProcessRequests(List<RequestObject> requests)
        {
            var responses = new List<ResponseObject>();
            foreach (var request in requests)
            {
                switch (request.method)
                {
                    case "list":
                        {
                            var node = SuperRoot.Get(request.path);
                            if (node != null)
                            {
                                StreamManager.Open(request.rid.Value, node);
                                responses.Add(new ResponseObject
                                {
                                    rid = request.rid,
                                    stream = "open",
                                    updates = SuperRoot.Get(request.path).Serialize()
                                });
                            }
                        }
                        break;
                    case "set":
                        {
                            var node = SuperRoot.Get(request.path);
                            if (node != null)
                            {
                                if (request.permit == null || request.permit.Equals(node.GetConfig("writable").Get())) {
                                    node.Value.Set(request.value);
                                    responses.Add(new ResponseObject
                                    {
                                        rid = request.rid,
                                        stream = "closed"
                                    });
                                }
                            }
                        }
                        break;
                    case "remove":
                        {
                            SuperRoot.RemoveConfigAttribute(request.path);
                            responses.Add(new ResponseObject
                            {
                                rid = request.rid,
                                stream = "closed"
                            });
                        }
                        break;
                    case "invoke":
                        {
                            var node = SuperRoot.Get(request.path);
                            if (node?.Action != null)
                            {
                                if (request.permit == null || request.permit.Equals(node.Action.Permission.ToString()))
                                {
                                    var parameters = request.@params.ToDictionary(pair => pair.Key, pair => new Value(pair.Value));
                                    var columns = node.GetConfig("columns") != null
                                        ? node.GetConfig("columns").Get()
                                        : new List<Column>();
                                    var permit = (request.permit != null) ? Permission._permMap[request.permit.ToLower()] : null;
                                    var invokeRequest = new InvokeRequest(request.rid.Value, request.path,
                                                                          permit, request.@params, link: _link,
                                                                          columns: columns);
                                    node.Action.Function.Invoke(parameters, invokeRequest);
                                }
                            }
                        }
                        break;
                    case "subscribe":
                        {
                            foreach (var pair in request.paths)
                            {
                                var node = SuperRoot.Get(pair.path);
                                if (node != null && pair.sid != null)
                                {
                                    SubscriptionManager.Subscribe(pair.sid.Value, SuperRoot.Get(pair.path));
                                    _link.Connector.Write(new RootObject
                                    {
                                        msg = _link.MessageId,
                                        responses = new List<ResponseObject>
                                        {
                                            new ResponseObject
                                            {
                                                rid = 0,
                                                updates = new List<dynamic>
                                                {
                                                    new[]
                                                    {
                                                        pair.sid.Value,
                                                        node.Value.Get(),
                                                        node.Value.LastUpdated
                                                    }
                                                }
                                            }
                                        }
                                    });
                                }
                            }
                            responses.Add(new ResponseObject
                            {
                                rid = request.rid,
                                stream = "closed"
                            });
                        }
                        break;
                    case "unsubscribe":
                        {
                            foreach (var sid in request.sids)
                            {
                                SubscriptionManager.Unsubscribe(sid);
                            }
                            responses.Add(new ResponseObject
                            {
                                rid = request.rid,
                                stream = "closed"
                            });
                        }
                        break;
                    case "close":
                        {
                            if (request.rid != null)
                            {
                                StreamManager.Close(request.rid.Value);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException(string.Format("Method {0} not implemented", request.method));
                }
            }
            return responses;
        }
    }

    /// <summary>
    /// Class to manage DSA subscriptions
    /// </summary>
    internal class SubscriptionManager
    {
        /// <summary>
        /// Map of subscription ID to a Node
        /// </summary>
        private readonly Dictionary<int, Node> _subscriptions = new Dictionary<int, Node>();

        /// <summary>
        /// DSLink container instance
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// SubscriptionManager constructor
        /// </summary>
        /// <param name="link"></param>
        public SubscriptionManager(AbstractContainer link)
        {
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
                _subscriptions[sid].Subscribers.Remove(sid);
                _subscriptions.Remove(sid);
            }
            catch (KeyNotFoundException)
            {
                _link.Logger.Info("Unknown rid");
            }
        }

        /// <summary>
        /// Ran when the connection is lost.
        /// Clears all subscriptions.
        /// </summary>
        internal void ClearAll()
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
            node.Streams.Add(requestId);
        }

        /// <summary>
        /// Close a stream to a Node with a request ID.
        /// </summary>
        /// <param name="requestId">Request ID</param>
        public void Close(int requestId)
        {
            try
            {
                _streams[requestId].Streams.Remove(requestId);
                _streams.Remove(requestId);
            }
            catch (KeyNotFoundException)
            {
                _link.Logger.Info("Unknown rid");
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
