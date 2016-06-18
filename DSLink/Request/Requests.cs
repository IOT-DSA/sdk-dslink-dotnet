using System;
using System.Collections.Generic;
using DSLink.Connection.Serializer;
using DSLink.Nodes;
using DSLink.Respond;

namespace DSLink.Request
{
    public abstract class BaseRequest
    {
        public int RequestID
        {
            get;
            private set;
        }

        protected BaseRequest(int requestID)
        {
            RequestID = requestID;
        }

        public abstract string Method();

        public virtual RequestObject Serialize()
        {
            return new RequestObject()
            {
                RequestId = RequestID,
                Method = Method()
            };
        }
    }

    public class ListRequest : BaseRequest
    {
        public Action<ListResponse> Callback
        {
            get;
        }

        public string Path
        {
            get;
        }

        public ListRequest(int requestID, Action<ListResponse> callback, string path) : base(requestID)
        {
            Callback = callback;
            Path = path;
        }

        public override string Method() => "list";

        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Path;
            return baseSerialized;
        }
    }
}
