using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using Newtonsoft.Json.Linq;

namespace DSLink.Respond
{
    /// <summary>
    /// Base response.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// DSLink container.
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// Request identifier for request.
        /// </summary>
        public int RequestID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Respond.Response"/> class.
        /// </summary>
        /// <param name="link">Link instance</param>
        /// <param name="requestID">Request identifier</param>
        public Response(AbstractContainer link, int requestID)
        {
            _link = link;
            RequestID = requestID;
        }

        /// <summary>
        /// Close the request.
        /// </summary>
        public async Task Close()
        {
            if (_link == null)
            {
                throw new NullReferenceException("Link is null, cannot close stream.");
            }
            _link.Requester._requestManager.StopRequest(RequestID);
            await _link.Connector.Write(new RootObject
            {
                Responses = new List<ResponseObject>
                {
                    new ResponseObject
                    {
                        RequestId = RequestID,
                        Stream = "closed"
                    }
                }
            });
        }
    }

    /// <summary>
    /// List response.
    /// </summary>
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
        public ListResponse(AbstractContainer link, int requestID,
                            string path, Node node)
            : base(link, requestID)
        {
            Path = path;
            Node = node;
        }
    }

    /// <summary>
    /// Invoke response.
    /// </summary>
    public class InvokeResponse : Response
    {
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
        public JArray Updates
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
        public InvokeResponse(AbstractContainer link, int requestID,
                              string path, List<Column> columns,
                              JArray updates)
            : base(link, requestID)
        {
            Path = path;
            Columns = columns;
            Updates = updates;
        }
    }
}
