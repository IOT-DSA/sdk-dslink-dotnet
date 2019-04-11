using System.Collections.Generic;

namespace DSLink.Request
{
    public class RequestManager
    {
        private readonly Dictionary<int, BaseRequest> _requests;

        public RequestManager()
        {
            _requests = new Dictionary<int, BaseRequest>();
        }

        public void StartRequest(BaseRequest request)
        {
            _requests.Add(request.RequestId, request);
        }

        public void StopRequest(int requestId)
        {
            _requests.Remove(requestId);
        }

        public bool RequestPending(int requestId)
        {
            return _requests.ContainsKey(requestId);
        }

        public BaseRequest GetRequest(int requestId)
        {
            return _requests[requestId];
        }
    }
}