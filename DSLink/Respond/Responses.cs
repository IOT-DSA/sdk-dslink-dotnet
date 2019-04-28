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
        private readonly BaseLinkHandler _link;

        /// <summary>
        /// Request identifier for request.
        /// </summary>
        private readonly int _requestId;

        protected Response(BaseLinkHandler link, int requestId)
        {
            _link = link;
            _requestId = requestId;
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

            _link.Requester.RequestManager.StopRequest(_requestId);
            await _link.Connection.Write(new JObject
            {
                new JProperty("responses", new JObject
                {
                    new JObject
                    {
                        new JProperty("rid", _requestId),
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
        public readonly string Path;

        /// <summary>
        /// Node of the list request.
        /// </summary>
        public readonly Node Node;

        public ListResponse(BaseLinkHandler link, int requestId,
            string path, Node node)
            : base(link, requestId)
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
        public readonly string Path;

        /// <summary>
        /// Columns from Response.
        /// </summary>
        public readonly JArray Columns;

        /// <summary>
        /// Updates from Response.
        /// </summary>
        public readonly JArray Updates;

        /// <summary>
        /// Metadata from the Response.
        /// </summary>
        public readonly JObject Meta;

        /// <summary>
        /// Error from the Response.
        /// </summary>
        public readonly JObject Error;

        /// <summary>
        /// True when Columns is neither true or 0.
        /// </summary>
        public bool HasColumns => Columns != null && Columns.Count > 0;

        /// <summary>
        /// True when Updates is neither true or 0;
        /// </summary>
        public bool HasUpdates => Updates.Count > 0;

        public InvokeResponse(BaseLinkHandler link, int requestId,
            string path, JArray columns,
            JArray updates, JObject meta, JObject error)
            : base(link, requestId)
        {
            Path = path;
            Columns = columns;
            Updates = updates;
            Meta = meta;
            Error = error;
        }
    }
}