using System.Collections.Generic;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Nodes.Actions;

namespace DSLink.Respond
{
    public class Response
    {
        /// <summary>
        /// Request identifier for request.
        /// </summary>
        public int RequestID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DSLink.Respond.Response"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        public Response(int requestID)
        {
            RequestID = requestID;
        }
    }

    public class ListResponse : Response
    {
        /// <summary>
        /// Path of the list request.
        /// </summary>
        public string Path
        {
            get;
            protected set;
        }

        /// <summary>
        /// Node of the list request.
        /// </summary>
        public Node Node
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DSLink.Respond.ListResponse"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="path">Path</param>
        /// <param name="node">Node</param>
        public ListResponse(int requestID, string path, Node node) : base(requestID)
        {
            Path = path;
            Node = node;
        }
    }

    public class InvokeResponse : Response
    {
        /// <summary>
        /// DSLink container.
        /// </summary>
        private AbstractContainer _link;

        /// <summary>
        /// Path of the Node.
        /// </summary>
        public string Path
        {
            get;
        }

        /// <summary>
        /// Columns from Response.
        /// </summary>
        public List<Column> Columns
        {
            get;
        }

        /// <summary>
        /// Updates from Response.
        /// </summary>
        public List<dynamic> Updates
        {
            get;
        }

        /// <summary>
        /// True when Columns is neither true or 0.
        /// </summary>
        public bool HasColumns => Columns != null && Columns.Count > 0;

        /// <summary>
        /// True when Updates is neither true or 0;
        /// </summary>
        public bool HasUpdates => Updates.Count > 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DSLink.Respond.InvokeResponse"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="path">Path</param>
        /// <param name="columns">Columns</param>
        /// <param name="updates">Updates</param>
        public InvokeResponse(AbstractContainer link, int requestID, string path, List<Column> columns, List<dynamic> updates) : base(requestID)
        {
            _link = link;
            Path = path;
            Columns = columns;
            Updates = updates;
        }

        /// <summary>
        /// Close the request.
        /// </summary>
        public void Close()
        {
            _link.Requester._requestManager.StopRequest(RequestID);
            _link.Connector.Write(new RootObject
            {
                Requests = new List<RequestObject>
                {
                    new RequestObject
                    {
                        RequestId = RequestID,
                        Method = "close"
                    }
                }
            });
        }
    }
}
