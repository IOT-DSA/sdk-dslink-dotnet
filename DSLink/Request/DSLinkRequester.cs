using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSLink.Nodes;
using DSLink.Respond;
using Newtonsoft.Json.Linq;

namespace DSLink.Request
{
    /// <summary>
    /// The requester module of a DSLink gives the ability access to
    /// outer data on the broker.
    /// </summary>
    public class DSLinkRequester
    {
        private readonly DSLinkContainer _link;
        internal readonly RequestManager _requestManager;
        internal readonly RemoteSubscriptionManager _remoteSubscriptionManager;
        private int _requestId = 1;
        private int NextRequestID => _requestId++;

        public DSLinkRequester(DSLinkContainer link)
        {
            _link = link;
            _requestManager = new RequestManager();
            _remoteSubscriptionManager = new RemoteSubscriptionManager(_link);
        }

        /// <summary>
        /// Send request to list a path.
        /// </summary>
        /// <param name="path">Remote path to list</param>
        /// <param name="callback">Callback event</param>
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
        /// Set the specified path's value.
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

            return await _remoteSubscriptionManager.Subscribe(path, callback, qos);
        }

        /// <summary>
        /// Unsubscribe from a subscription ID.
        /// </summary>
        /// <param name="path">Subscription ID</param>
        public async Task Unsubscribe(int subId)
        {
            await _remoteSubscriptionManager.Unsubscribe(subId);
        }

        internal async Task<JArray> ProcessResponses(JArray responses)
        {
            var requests = new JArray();

            foreach (JObject response in responses)
            {
                await ProcessResponse(response);
            }

            return requests;
        }

        private async Task ProcessResponse(JObject response)
        {
            if (response["rid"] == null || response["rid"].Type != JTokenType.Integer)
            {
                _link.Logger.Warning("Incoming request has invalid or null request ID.");
                return;
            }

            int rid = response["rid"].Value<int>();

            if (rid == 0)
            {
                ProcessValueUpdates(response);
            }
            else if (_requestManager.RequestPending(rid))
            {
                await ProcessRequestUpdates(response, rid);
            }
        }

        private void ProcessValueUpdates(JObject response)
        {
            foreach (dynamic update in response["updates"])
            {
                if (update is JArray)
                {
                    ProcessUpdateArray(update);
                }
                else if (update is JObject)
                {
                    ProcessUpdateObject(update);
                }
            }
        }

        private void ProcessUpdateArray(dynamic update)
        {
            JArray arrayUpdate = update;
            int sid = arrayUpdate[0].Value<int>();
            JToken value = arrayUpdate[1];
            string dt = arrayUpdate[2].Value<string>();
            _remoteSubscriptionManager.InvokeSubscriptionUpdate(sid, new SubscriptionUpdate(sid, value, dt));
        }

        private void ProcessUpdateObject(dynamic update)
        {
            JObject objectUpdate = update;
            int sid = objectUpdate["sid"].Value<int>();
            JToken value = objectUpdate["value"];
            string ts = objectUpdate["ts"].Value<string>();
            int count = objectUpdate["count"].Value<int>();
            int sum = objectUpdate["sum"].Value<int>();
            int min = objectUpdate["min"].Value<int>();
            int max = objectUpdate["max"].Value<int>();
            _remoteSubscriptionManager.InvokeSubscriptionUpdate(sid, new SubscriptionUpdate(sid, value, ts, count, sum, min, max));
        }

        private async Task ProcessRequestUpdates(JObject response, int rid)
        {
            var request = _requestManager.GetRequest(rid);
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

        internal class RequestManager
        {
            private readonly Dictionary<int, BaseRequest> _requests;

            public RequestManager()
            {
                _requests = new Dictionary<int, BaseRequest>();
            }

            public void StartRequest(BaseRequest request)
            {
                _requests.Add(request.RequestID, request);
            }

            public void StopRequest(int requestID)
            {
                _requests.Remove(requestID);
            }

            public bool RequestPending(int requestID)
            {
                return _requests.ContainsKey(requestID);
            }

            public BaseRequest GetRequest(int requestID)
            {
                return _requests[requestID];
            }
        }

        internal class RemoteSubscriptionManager
        {
            private readonly DSLinkContainer _link;
            private readonly Dictionary<string, Subscription> _subscriptions;
            private readonly Dictionary<int, string> _subIdToPath;
            private readonly Dictionary<int, string> _realSubIdToPath;
            private int _subscriptionId = 0;
            
            public RemoteSubscriptionManager(DSLinkContainer link)
            {
                _link = link;
                _subscriptions = new Dictionary<string, Subscription>();
                _subIdToPath = new Dictionary<int, string>();
                _realSubIdToPath = new Dictionary<int, string>();
            }

            public async Task<int> Subscribe(string path, Action<SubscriptionUpdate> callback, int qos)
            {
                var sid = _subscriptionId++;
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
