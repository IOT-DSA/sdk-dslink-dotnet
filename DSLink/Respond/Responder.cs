using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        internal async Task<List<ResponseObject>> ProcessRequests(List<RequestObject> requests)
        {
            var responses = new List<ResponseObject>();
            foreach (var request in requests)
            {
                switch (request.Method)
                {
                    case "list":
                        {
                            var node = SuperRoot.Get(request.Path);
                            if (node != null)
                            {
                                StreamManager.Open(request.RequestId.Value, node);
                                responses.Add(new ResponseObject
                                {
                                    RequestId = request.RequestId,
                                    Stream = "open",
                                    Updates = SuperRoot.Get(request.Path).Serialize()
                                });
                            }
                        }
                        break;
                    case "set":
                        {
                            var node = SuperRoot.Get(request.Path);
                            if (node != null)
                            {
                                if (request.Permit == null || request.Permit.Equals(node.GetConfig("writable").Get()))
                                {
                                    node.Value.Set(request.Value);
                                    responses.Add(new ResponseObject
                                    {
                                        RequestId = request.RequestId,
                                        Stream = "closed"
                                    });
                                }
                            }
                        }
                        break;
                    case "remove":
                        {
                            SuperRoot.RemoveConfigAttribute(request.Path);
                            responses.Add(new ResponseObject
                            {
                                RequestId = request.RequestId,
                                Stream = "closed"
                            });
                        }
                        break;
                    case "invoke":
                        {
                            var node = SuperRoot.Get(request.Path);
                            if (node?.Action != null)
                            {
                                if (request.Permit == null || request.Permit.Equals(node.Action.Permission.ToString()))
                                {
                                    var parameters = request.Parameters.ToDictionary(pair => pair.Key, pair => new Value(pair.Value));
                                    var columns = node.GetConfig("columns") != null
                                        ? node.GetConfig("columns").Get()
                                        : new List<Column>();
                                    var permit = (request.Permit != null) ? Permission._permMap[request.Permit.ToLower()] : null;
                                    var invokeRequest = new InvokeRequest(request.RequestId.Value, request.Path,
                                                                          permit, request.Parameters, link: _link,
                                                                          columns: columns);
                                    await Task.Run(() => node.Action.Function.Invoke(parameters, invokeRequest));
                                }
                            }
                        }
                        break;
                    case "subscribe":
                        {
                            foreach (var pair in request.Paths)
                            {
                                var node = SuperRoot.Get(pair.Path);
                                if (node != null && pair.SubscriptionId != null)
                                {
                                    SubscriptionManager.Subscribe(pair.SubscriptionId.Value, SuperRoot.Get(pair.Path));
                                    responses.Add(new ResponseObject
                                    {
                                        RequestId = 0,
                                        Updates = new List<dynamic>
                                        {
                                            new List<dynamic>
                                            {
                                                pair.SubscriptionId.Value,
                                                node.Value.Get(),
                                                node.Value.LastUpdated
                                            }
                                        }
                                    });
                                }
                            }
                            responses.Add(new ResponseObject
                            {
                                RequestId = request.RequestId,
                                Stream = "closed"
                            });
                        }
                        break;
                    case "unsubscribe":
                        {
                            foreach (var sid in request.SubscriptionIds)
                            {
                                SubscriptionManager.Unsubscribe(sid);
                            }
                            responses.Add(new ResponseObject
                            {
                                RequestId = request.RequestId,
                                Stream = "closed"
                            });
                        }
                        break;
                    case "close":
                        {
                            if (request.RequestId != null)
                            {
                                StreamManager.Close(request.RequestId.Value);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException(string.Format("Method {0} not implemented", request.Method));
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
