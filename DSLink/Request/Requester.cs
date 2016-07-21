using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Respond;
using Newtonsoft.Json.Linq;

namespace DSLink.Request
{
    /// <summary>
    /// Requester module.
    /// </summary>
    public class Requester
    {
        /// <summary>
        /// Link container.
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// Request manager.
        /// </summary>
        internal readonly RequestManager _requestManager;

        /// <summary>
        /// Remote subscription manager.
        /// </summary>
        internal readonly RemoteSubscriptionManager _subscriptionManager;

        /// <summary>
        /// Current request ID.
        /// </summary>
        private int _rid = 1;

        /// <summary>
        /// Current subscription ID.
        /// </summary>
        private int _sid = 0;

        /// <summary>
        /// Gets the next request identifier.
        /// </summary>
        protected int NextRequestID => _rid++;

        /// <summary>
        /// Gets the next subscription identifier.
        /// </summary>
        protected int NextSubID => _sid++;

        internal Requester(AbstractContainer link)
        {
            _link = link;
            _requestManager = new RequestManager();
            _subscriptionManager = new RemoteSubscriptionManager(_link);
        }

        /// <summary>
        /// Processes incoming responses.
        /// </summary>
        /// <param name="responses">Responses</param>
        /// <returns>Requests</returns>
        internal async Task<List<RequestObject>> ProcessResponses(List<ResponseObject> responses)
        {
            var requests = new List<RequestObject>();

            foreach (var response in responses)
            {
                if (response.RequestId.HasValue && response.RequestId.Value == 0)
                {
                    foreach (dynamic update in response.Updates)
                    {
                        if (update is JArray)
                        {
                            JArray arrayUpdate = update;
                            int sid = arrayUpdate[0].Value<int>();
                            JToken value = arrayUpdate[1];
                            string dt = arrayUpdate[2].Value<string>();
                            var sub = _subscriptionManager.GetSub(sid);
                            if (sub != null)
                            {
                                await Task.Run(() => sub.Callback(new SubscriptionUpdate(sid, value, dt)));
                            }
                        }
                        else if (update is JObject)
                        {
                            JObject objectUpdate = update;
                            int sid = objectUpdate["sid"].Value<int>();
                            JToken value = objectUpdate["value"];
                            string ts = objectUpdate["ts"].Value<string>();
                            int count = objectUpdate["count"].Value<int>();
                            int sum = objectUpdate["sum"].Value<int>();
                            int min = objectUpdate["min"].Value<int>();
                            int max = objectUpdate["max"].Value<int>();
                            var sub = _subscriptionManager.GetSub(sid);
                            if (sub != null)
                            {
                                await Task.Run(() => sub.Callback(new SubscriptionUpdate(sid, value, ts, count, sum, min, max)));
                            }
                        }
                    }
                }
                else if (response.RequestId.HasValue && _requestManager.RequestPending(response.RequestId.Value))
                {
                    var request = _requestManager.GetRequest(response.RequestId.Value);
                    if (request is ListRequest)
                    {
                        var listRequest = request as ListRequest;
                        string name = listRequest.Path.Split('/').Last();
                        var node = new RemoteNode(name, null, listRequest.Path);
                        node.FromSerialized(response.Updates);
                        await Task.Run(() => listRequest.Callback(new ListResponse(_link, listRequest.RequestID,
                                                                                   listRequest.Path, node)));
                    }
                    else if (request is SetRequest)
                    {
                        _requestManager.StopRequest(request.RequestID);
                    }
                    else if (request is RemoveRequest)
                    {
                        _requestManager.StopRequest(request.RequestID);
                    }
                    else if (request is InvokeRequest)
                    {
                        var invokeRequest = request as InvokeRequest;
                        await Task.Run(() => invokeRequest.Callback(new InvokeResponse(_link, invokeRequest.RequestID,
                                                                                       invokeRequest.Path, response.Columns,
                                                                                       response.Updates)));
                    }
                }
                else if (response.RequestId == null)
                {
                    _link.Logger.Warning("Incoming request has null request ID.");
                }
            }

            return requests;
        }

        /// <summary>
        /// List the specified path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="callback">Callback</param>
        public async Task<ListRequest> List(string path, Action<ListResponse> callback)
        {
            var request = new ListRequest(NextRequestID, callback, path, _link);
            _requestManager.StartRequest(request);
            await _link.Connector.Write(new RootObject
            {
                Requests = new List<RequestObject>
                {
                    request.Serialize()
                }
            });
            return request;
        }

        /// <summary>
        /// Set the specified path value.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="permission">Permission</param>
        /// <param name="value">Value</param>
        public async Task<SetRequest> Set(string path, Permission permission, Value value)
        {
            var request = new SetRequest(NextRequestID, path, permission, value);
            _requestManager.StartRequest(request);
            await _link.Connector.Write(new RootObject
            {
                Requests = new List<RequestObject>
                {
                    request.Serialize()
                }
            });
            return request;
        }

        /// <summary>
        /// Remove the specified path.
        /// </summary>
        /// <param name="path">Path</param>
        public async Task<RemoveRequest> Remove(string path)
        {
            var request = new RemoveRequest(NextRequestID, path);
            _requestManager.StartRequest(request);
            await _link.Connector.Write(new RootObject
            {
                Requests = new List<RequestObject>
                {
                    request.Serialize()
                }
            });
            return request;
        }

        /// <summary>
        /// Invoke the specified path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="permission">Permission</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="callback">Callback</param>
        public async Task<InvokeRequest> Invoke(string path, Permission permission, Dictionary<string, JToken> parameters, Action<InvokeResponse> callback)
        {
            var request = new InvokeRequest(NextRequestID, path, permission, parameters, callback);
            _requestManager.StartRequest(request);
            await _link.Connector.Write(new RootObject
            {
                Requests = new List<RequestObject>
                {
                    request.Serialize()
                }
            });
            return request;
        }

        /// <summary>
        /// Subscribe to the specified path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="callback">Callback</param>
        /// <param name="qos">Quality of Service</param>
        public async Task<SubscribeRequest> Subscribe(string path, Action<SubscriptionUpdate> callback, int qos = 0)
        {
            var sid = NextSubID;
            var request = new SubscribeRequest(NextRequestID, new List<AddSubscriptionObject>
            {
                new AddSubscriptionObject
                {
                    Path = path,
                    SubscriptionId = sid,
                    QualityOfService = qos
                }
            }, callback);

            _subscriptionManager.Subscribe(sid, path, callback);
            await _link.Connector.Write(new RootObject
            {
                Requests = new List<RequestObject>
                {
                    request.Serialize()
                }
            });

            return request;
        }

        /// <summary>
        /// Unsubscribe from the specified path.
        /// </summary>
        /// <param name="path">Path</param>
        public void Unsubscribe(string path)
        {
            Unsubscribe(new List<string>
            {
                path
            });
        }

        /// <summary>
        /// Unsubscribe from the specified paths.
        /// </summary>
        /// <param name="paths">List of paths</param>
        public void Unsubscribe(List<string> paths)
        {
            var sids = new List<int>();
            foreach (string path in paths)
            {
                var sid = _subscriptionManager.GetSubByPath(path);
                if (sid.HasValue)
                {
                    _subscriptionManager.Unsubscribe(sid.Value);
                    sids.Add(sid.Value);
                }
            }
            if (sids.Count > 0)
            {
                _link.Connector.Write(new RootObject
                {
                    Requests = new List<RequestObject>
                    {
                        new UnsubscribeRequest(NextRequestID, sids).Serialize()
                    }
                });
            }
        }

        /// <summary>
        /// Request manager handles outgoing requests.
        /// </summary>
        internal class RequestManager
        {
            /// <summary>
            /// Dictionary of requests mapped to request IDs.
            /// </summary>
            private readonly Dictionary<int, BaseRequest> requests;

            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="T:DSLink.Request.Requester.RequestManager"/> class.
            /// </summary>
            public RequestManager()
            {
                requests = new Dictionary<int, BaseRequest>();
            }

            /// <summary>
            /// Starts a request.
            /// </summary>
            /// <param name="request">Request</param>
            public void StartRequest(BaseRequest request)
            {
                requests.Add(request.RequestID, request);
            }

            /// <summary>
            /// Stops a request.
            /// </summary>
            /// <param name="requestID">Request identifier.</param>
            public void StopRequest(int requestID)
            {
                requests.Remove(requestID);
            }

            /// <summary>
            /// Determines if a request is pending
            /// </summary>
            /// <param name="requestID">Request identifier</param>
            public bool RequestPending(int requestID)
            {
                return requests.ContainsKey(requestID);
            }

            /// <summary>
            /// Gets the request for a request ID.
            /// </summary>
            /// <param name="requestID">Request identifier</param>
            public BaseRequest GetRequest(int requestID)
            {
                return requests[requestID];
            }
        }

        /// <summary>
        /// Remote subscription manager.
        /// </summary>
        internal class RemoteSubscriptionManager
        {
            /// <summary>
            /// DSLink container link instance.
            /// </summary>
            private readonly AbstractContainer _link;

            /// <summary>
            /// Subscription IDs to Subscription objects.
            /// </summary>
            private readonly Dictionary<int, Subscription> _subscriptions;

            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="T:DSLink.Request.Requester.RemoteSubscriptionManager"/> class.
            /// </summary>
            public RemoteSubscriptionManager(AbstractContainer link)
            {
                _link = link;
                _subscriptions = new Dictionary<int, Subscription>();
            }

            /// <summary>
            /// Subscribe to the specified subscription ID.
            /// </summary>
            /// <param name="subID">Subscription ID</param>
            /// <param name="path">Path</param>
            /// <param name="callback">Callback</param>
            public void Subscribe(int subID, string path, Action<SubscriptionUpdate> callback)
            {
                if (_subscriptions.ContainsKey(subID))
                {
                    throw new Exception("Subscription ID is already used");
                }
                _subscriptions.Add(subID, new Subscription
                {
                    Path = path,
                    Callback = callback
                });
            }

            /// <summary>
            /// Unsubscribe from the specified subscription ID.
            /// </summary>
            /// <param name="subID">Subscription ID</param>
            public void Unsubscribe(int subID)
            {
                _subscriptions.Remove(subID);
            }

            /// <summary>
            /// Get subscription ID from path.
            /// </summary>
            /// <param name="path">Path of node</param>
            /// <returns>Subscription ID</returns>
            public int? GetSubByPath(string path)
            {
                foreach (KeyValuePair<int, Subscription> kp in _subscriptions)
                {
                    if (kp.Value.Path == path)
                    {
                        return kp.Key;
                    }
                }
                return null;
            }

            /// <summary>
            /// Get the subscription object from a subscription ID.
            /// </summary>
            /// <param name="subID">Subscription ID</param>
            /// <returns>Subscription object</returns>
            public Subscription GetSub(int subID)
            {
                if (!_subscriptions.ContainsKey(subID))
                {
                    _link.Logger.Debug(string.Format("Remote sid {0} was not found in subscription manager", subID));
                    return null;
                }
                return _subscriptions[subID];
            }

            /// <summary>
            /// Subscription object that maps a path and callback to an ID.
            /// </summary>
            public class Subscription
            {
                public string Path;
                public Action<SubscriptionUpdate> Callback;
            }
        }
    }
}
