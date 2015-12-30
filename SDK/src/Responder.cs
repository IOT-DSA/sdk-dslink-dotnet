using System;
using System.Collections.Generic;
using System.Linq;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Nodes.Actions;

namespace DSLink
{
    public sealed class Responder
    {
        private readonly AbstractContainer _link;
        public Node SuperRoot { get; }
        internal SubscriptionManager SubscriptionManager;
        internal StreamManager StreamManager;

        internal Responder(AbstractContainer link)
        {
            _link = link;
            SuperRoot = new Node("", null, _link);
            SubscriptionManager = new SubscriptionManager(_link);
            StreamManager = new StreamManager(_link);
        }

        internal List<ResponseObject> ProcessRequests(List<RequestObject> requests)
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
                                if (request.Permit == null || request.Permit.Equals(node.GetConfig("writable").Get())) {
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
                                    var updateValues = node.Action.Function.Invoke(parameters);
                                    var updates = updateValues.Select(value => new[] {value}).Cast<dynamic>().ToList();
                                    var columns = node.GetConfig("columns") != null
                                        ? node.GetConfig("columns").Get()
                                        : new List<Column>();
                                    responses.Add(new ResponseObject
                                    {
                                        RequestId = request.RequestId,
                                        Stream = "closed",
                                        Columns = columns,
                                        Updates = updates
                                    });
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
                                    _link.Connector.Write(new RootObject
                                    {
                                        Msg = _link.MessageId,
                                        Responses = new List<ResponseObject>
                                        {
                                            new ResponseObject
                                            {
                                                RequestId = 0,
                                                Updates = new List<dynamic>
                                                {
                                                    new[]
                                                    {
                                                        pair.SubscriptionId.Value,
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
                        throw new ArgumentException("Method not implemented");
                }
            }
            return responses;
        }
    }

    internal class SubscriptionManager
    {
        private readonly Dictionary<int, Node> _subscriptions = new Dictionary<int, Node>();
        private readonly AbstractContainer _link;

        public SubscriptionManager(AbstractContainer link)
        {
            _link = link;
        }

        public void Subscribe(int sid, Node node)
        {
            node.Subscribers.Add(sid);
            _subscriptions.Add(sid, node);
        }

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
    }

    internal class StreamManager
    {
        private readonly Dictionary<int, Node> _streams = new Dictionary<int, Node>();
        private readonly AbstractContainer _link;

        public StreamManager(AbstractContainer link)
        {
            _link = link;
        }

        public void Open(int requestId, Node node)
        {
            _streams.Add(requestId, node);
            node.Streams.Add(requestId);
        }

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
    }
}
