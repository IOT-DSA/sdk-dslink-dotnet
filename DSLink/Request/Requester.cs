using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        /// Gets the next request identifier.
        /// </summary>
        protected int NextRequestID => _rid++;

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
        internal async Task<JArray> ProcessResponses(JArray responses)
        {
            var requests = new JArray();

            foreach (JObject response in responses)
            {
                if (response["rid"].Type == JTokenType.Integer && response["rid"].Value<int>() == 0)
                {
                    foreach (dynamic update in response["updates"])
                    {
                        if (update is JArray)
                        {
                            JArray arrayUpdate = update;
                            int sid = arrayUpdate[0].Value<int>();
                            JToken value = arrayUpdate[1];
                            string dt = arrayUpdate[2].Value<string>();
                            _subscriptionManager.InvokeSubscriptionUpdate(sid, new SubscriptionUpdate(sid, value, dt));
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
                            _subscriptionManager.InvokeSubscriptionUpdate(sid, new SubscriptionUpdate(sid, value, ts, count, sum, min, max));
                        }
                    }
                }
                else if (response["rid"].Type == JTokenType.Integer && _requestManager.RequestPending(response["rid"].Value<int>()))
                {
                    var request = _requestManager.GetRequest(response["rid"].Value<int>());
                    if (request is ListRequest)
                    {
                        var listRequest = request as ListRequest;
                        string name = listRequest.Path.Split('/').Last();
                        var node = new RemoteNode(name, null, listRequest.Path);
                        node.FromSerialized(response["updates"].Value<JArray>());
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
                                                                                       invokeRequest.Path, response["columns"].Value<JArray>(),
                                                                                       response["updates"].Value<JArray>())));
                    }
                }
                else if (response["rid"].Type == JTokenType.Null)
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
            await _link.Connector.Write(new JObject
            {
                new JProperty("requests", new JArray
                {
                    request.Serialize()
                })
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
            await _link.Connector.Write(new JObject
            {
                new JProperty("requests", new JArray
                {
                    request.Serialize()
                })
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
            await _link.Connector.Write(new JObject
            {
                new JProperty("requests", new JArray
                {
                    request.Serialize()
                })
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
        public async Task<InvokeRequest> Invoke(string path, Permission permission, JObject parameters, Action<InvokeResponse> callback)
        {
            var request = new InvokeRequest(NextRequestID, path, permission, parameters, callback);
            _requestManager.StartRequest(request);
            await _link.Connector.Write(new JObject
            {
                new JProperty("requests", new JArray
                {
                    request.Serialize()
                })
            });
            return request;
        }

        /// <summary>
        /// Subscribe to the specified path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="callback">Callback</param>
        /// <param name="qos">Quality of Service</param>
        public async Task<int> Subscribe(string path, Action<SubscriptionUpdate> callback, int qos = 0)
        {
            // TODO: Test for quality of service changes.

            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path can not be null or empty.");
            }

            return await _subscriptionManager.Subscribe(path, callback, qos);
        }

        /// <summary>
        /// Unsubscribe from a subscription ID.
        /// </summary>
        /// <param name="path">Subscription ID</param>
        public async Task Unsubscribe(int subId)
        {
            await _subscriptionManager.Unsubscribe(subId);
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
            /// Storage of subscription objects, by node path.
            /// </summary>
            private readonly Dictionary<string, Subscription> _subscriptions;

            /// <summary>
            /// Direct mapping of subscription ID(virtual) to 
            /// node path.
            /// </summary>
            private readonly Dictionary<int, string> _subIdToPath;

            /// <summary>
            /// Real sub IDs to node paths.
            /// </summary>
            private readonly Dictionary<int, string> _realSubIdToPath;

            /// <summary>
            /// Current subscription ID.
            /// </summary>
            private int _sid = 0;

            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="T:DSLink.Request.Requester.RemoteSubscriptionManager"/> class.
            /// </summary>
            public RemoteSubscriptionManager(AbstractContainer link)
            {
                _link = link;
                _subscriptions = new Dictionary<string, Subscription>();
                _subIdToPath = new Dictionary<int, string>();
                _realSubIdToPath = new Dictionary<int, string>();
            }

            /// <summary>
            /// Subscribe to specified path.
            /// </summary>
            /// <param name="path">Path</param>
            /// <param name="callback">Callback</param>
            public async Task<int> Subscribe(string path, Action<SubscriptionUpdate> callback, int qos)
            {
                var sid = _sid++;
                var request = new SubscribeRequest(_link.Requester.NextRequestID, new JArray
                {
                    new JObject
                    {
                        new JProperty("path", path),
                        new JProperty("sid", sid),
                        new JProperty("qos", qos)
                    }
                }, callback);
                if (!_subscriptions.ContainsKey(path))
                {
                    _subscriptions.Add(path, new Subscription(sid));
                    await _link.Connector.Write(new JObject
                    {
                        new JProperty("requests", new JArray
                        {
                            request.Serialize()
                        })
                    });
                    _realSubIdToPath[sid] = path;
                }
                _subscriptions[path].VirtualSubs[sid] = callback;
                _subIdToPath[sid] = path;

                return sid;
            }

            /// <summary>
            /// Unsubscribe from the specified subscription ID.
            /// </summary>
            /// <param name="subId">Subscription ID</param>
            public async Task Unsubscribe(int subId)
            {
                var path = _subIdToPath[subId];
                var sub = _subscriptions[path];
                sub.VirtualSubs.Remove(subId);
                _subIdToPath.Remove(subId);
                if (sub.VirtualSubs.Count == 0)
                {
                    await _link.Connector.Write(new JObject
                    {
                        new JProperty("requests", new JArray
                        {
                            new UnsubscribeRequest(_link.Requester.NextRequestID, new JArray { sub.RealSubID }).Serialize()
                        })
                    });
                    _subscriptions.Remove(path);
                    _subIdToPath.Remove(sub.RealSubID);
                    _realSubIdToPath.Remove(sub.RealSubID);
                }
            }

            /// <summary>
            /// Get subscription IDs from path.
            /// </summary>
            /// <param name="path">Path of node</param>
            /// <returns>Subscription IDs in List</returns>
            public List<int> GetSubsByPath(string path)
            {
                var sids = new List<int>();

                if (_subscriptions.ContainsKey(path))
                {
                    foreach (var sid in _subscriptions[path].VirtualSubs)
                    {
                        sids.Add(sid.Key);
                    }
                }

                return sids;
            }

            /// <summary>
            /// Invoke subscription update callbacks.
            /// </summary>
            /// <param name="subId">Subscription ID</param>
            /// <param name="update">Update to send</param>
            public void InvokeSubscriptionUpdate(int subId, SubscriptionUpdate update)
            {
                if (!_realSubIdToPath.ContainsKey(subId))
                {
                    _link.Logger.Debug(string.Format("Remote sid {0} was not found in subscription manager", subId));
                    return;
                }
                foreach (var i in _subscriptions[_realSubIdToPath[subId]].VirtualSubs)
                {
                    i.Value(update);
                }
            }

            public class Subscription
            {
                public Subscription(int subId)
                {
                    RealSubID = subId;
                }

                public readonly int RealSubID;
                public readonly Dictionary<int, Action<SubscriptionUpdate>> VirtualSubs = new Dictionary<int, Action<SubscriptionUpdate>>();
            }
        }
    }
}
