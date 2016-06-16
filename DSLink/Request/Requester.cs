using System;
using System.Collections.Generic;
using System.Diagnostics;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Request;

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

            }

            return requests;
        }

        public void List(string path, Action<List<Node>> callback)
        {
            var metadata = new Dictionary<string, dynamic> {
                { "path", path }
            };
            var request = new ListRequest(NextRequestID, (List<Node> nodes) =>
            {
                foreach (Node node in nodes)
                {
                    Debug.WriteLine(node.Name);
                }
            }, metadata);
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
        }
    }
}
