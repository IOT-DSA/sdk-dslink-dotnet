using System;
using System.Collections.Generic;
using DSLink.Connection.Serializer;
using DSLink.Nodes;

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
        public Action<List<Node>> Callback
        {
            get;
        }

        public Dictionary<string, dynamic> Metadata
        {
            get;
        }

        public ListRequest(int requestID, Action<List<Node>> callback, Dictionary<string, dynamic> metadata) : base(requestID)
        {
            Callback = callback;
            Metadata = metadata;
        }

        public override string Method() => "list";

        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Metadata["path"];
            return baseSerialized;
        }
    }
}
