using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;
using DSLink.Respond;

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
        }

        /// <summary>
        /// Processes incoming requests.
        /// </summary>
        /// <param name="responses">Responses</param>
        /// <returns>Requests</returns>
        internal List<RequestObject> ProcessRequests(List<ResponseObject> responses)
        {
            var requests = new List<RequestObject>();

            foreach (var response in responses)
            {
                if (response.RequestId != null && _requestManager.RequestPending(response.RequestId.Value))
                {
                    var request = _requestManager.GetRequest(response.RequestId.Value);
                    if (request is ListRequest)
                    {
                        var listRequest = request as ListRequest;
                        string name = listRequest.Path.Split('/').Last();
                        var node = new RemoteNode(name, null);
                        node.FromSerialized(response.Updates);
                        listRequest.Callback(new ListResponse(listRequest.RequestID, listRequest.Path, node));
                        _requestManager.StopRequest(listRequest.RequestID);
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
                        invokeRequest.Callback(new InvokeResponse(_link, invokeRequest.RequestID, invokeRequest.Path, response.Columns, response.Updates));
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
        public ListRequest List(string path, Action<ListResponse> callback)
        {
            var request = new ListRequest(NextRequestID, callback, path);
            _requestManager.StartRequest(request);
            _link.Connector.Write(new RootObject
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
        public SetRequest Set(string path, Permission permission, Value value)
        {
            var request = new SetRequest(NextRequestID, path, permission, value);
            _requestManager.StartRequest(request);
            _link.Connector.Write(new RootObject
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
        public RemoveRequest Remove(string path)
        {
            var request = new RemoveRequest(NextRequestID, path);
            _requestManager.StartRequest(request);
            _link.Connector.Write(new RootObject
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
        public InvokeRequest Invoke(string path, Permission permission, Dictionary<string, dynamic> parameters, Action<InvokeResponse> callback)
        {
            var request = new InvokeRequest(NextRequestID, path, permission, parameters, callback);
            _requestManager.StartRequest(request);
            _link.Connector.Write(new RootObject
            {
                Requests = new List<RequestObject>
                {
                    request.Serialize()
                }
            });
            return request;
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
    }
}
