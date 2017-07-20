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
}
