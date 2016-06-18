using System.Collections.Generic;
using DSLink.Nodes;

namespace DSLink.Respond
{
    public class Response
    {
        public int RequestID
        {
            get;
            protected set;
        }

        public Response(int requestID)
        {
            RequestID = requestID;
        }
    }

    public class ListResponse : Response
    {
        public string Path
        {
            get;
            protected set;
        }

        public Node Node
        {
            get;
            protected set;
        }

        public ListResponse(int requestID, string path, Node node) : base(requestID)
        {
            Path = path;
            Node = node;
        }
    }
}
