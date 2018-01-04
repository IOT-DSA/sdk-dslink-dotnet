using System;
using System.Threading.Tasks;
using DSLink.Nodes;
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
        private readonly DSLinkContainer _link;

        /// <summary>
        /// Request identifier for request.
        /// </summary>
        public int RequestID
        {
            get;
            protected set;
        }

        public Response(DSLinkContainer link, int requestID)
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
            _link.Requester.RequestManager.StopRequest(RequestID);
            await _link.Connector.Write(new JObject
            {
                new JProperty("responses", new JObject
                {
                    new JObject
                    {
                        new JProperty("rid", RequestID),
                        new JProperty("stream", "closed")
                    }
                })
            });
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

        public ListResponse(DSLinkContainer link, int requestID,
                            string path, Node node)
            : base(link, requestID)
        {
            Path = path;
            Node = node;
        }
    }

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
        public JArray Columns
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

        public InvokeResponse(DSLinkContainer link, int requestID,
                              string path, JArray columns,
                              JArray updates)
            : base(link, requestID)
        {
            Path = path;
            Columns = columns;
            Updates = updates;
        }
    }
}
