using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Request;
using DSLink.Respond;

namespace DSLink.Request
{
    public class Requester
    {
        private readonly AbstractContainer _link;
        private readonly RequestManager _requestManager;
        private int _rid = 1;
        protected int NextRequestID => _rid++;

        internal Requester(AbstractContainer link)
        {
            _link = link;
            _requestManager = new RequestManager();
        }

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
                        RemoteNode node = new RemoteNode(name, null);
                        node.FromSerialized(response.Updates);
                        listRequest.Callback(new ListResponse(listRequest.RequestID, listRequest.Path, node));
                    }
                }
                else if (response.RequestId == null)
                {
                    _link.Logger.Warning("Incoming request has null request ID.");
                }
            }

            return requests;
        }

        public void List(string path, Action<ListResponse> callback)
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
        }

        private class RequestManager
        {
            private Dictionary<int, BaseRequest> requests;

            public RequestManager()
            {
                requests = new Dictionary<int, BaseRequest>();
            }

            public void StartRequest(BaseRequest request)
            {
                requests.Add(request.RequestID, request);
            }

            public void StopRequest(int requestID)
            {
                requests.Remove(requestID);
            }

            public bool RequestPending(int requestID)
            {
                return requests.ContainsKey(requestID);
            }

            public BaseRequest GetRequest(int requestID)
            {
                return requests[requestID];
            }
        }
    }
}
