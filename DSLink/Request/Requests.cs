using System;
using System.Threading.Tasks;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Respond;
using Newtonsoft.Json.Linq;

namespace DSLink.Request
{
    /// <summary>
    /// Base request object.
    /// </summary>
    public abstract class BaseRequest
    {
        /// <summary>
        /// Request identifier for the request.
        /// </summary>
        public readonly int RequestId;

        protected BaseRequest(int requestId)
        {
            RequestId = requestId;
        }

        /// <summary>
        /// Method of the request.
        /// </summary>
        public virtual string Method => "";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public virtual JObject Serialize()
        {
            return new JObject
            {
                new JProperty("rid", RequestId),
                new JProperty("method", Method)
            };
        }
    }

    /// <summary>
    /// List request object.
    /// </summary>
    public class ListRequest : BaseRequest
    {
        /// <summary>
        /// Callback that is called when response is received.
        /// </summary>
        public readonly Action<ListResponse> Callback;

        /// <summary>
        /// Path of the request.
        /// </summary>
        public readonly string Path;

        public ListRequest(int requestId, Action<ListResponse> callback, string path) : base(requestId)
        {
            Callback = callback;
            Path = path;
        }

        /// <summary>
        /// Request method.
        /// </summary>
        public override string Method => "list";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override JObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized["path"] = Path;
            return baseSerialized;
        }
    }

    /// <summary>
    /// Set request object.
    /// </summary>
    public class SetRequest : BaseRequest
    {
        /// <summary>
        /// Path to run the request with.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Permission of the request.
        /// </summary>
        public readonly Permission Permission;

        /// <summary>
        /// Value of the request.
        /// </summary>
        public readonly Value Value;

        public SetRequest(int requestId, string path, Permission permission, Value value) : base(requestId)
        {
            Path = path;
            Permission = permission;
            Value = value;
        }

        /// <inheritdoc />
        public override string Method => "set";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override JObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized["path"] = Path;
            baseSerialized["permit"] = Permission.ToString();
            baseSerialized["value"] = Value.JToken;
            return baseSerialized;
        }
    }

    /// <summary>
    /// Remove request object.
    /// </summary>
    public class RemoveRequest : BaseRequest
    {
        /// <summary>
        /// Path of the request.
        /// </summary>
        public readonly string Path;

        public RemoveRequest(int requestId, string path) : base(requestId)
        {
            Path = path;
        }

        /// <inheritdoc />
        public override string Method => "remove";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override JObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized["path"] = Path;
            return baseSerialized;
        }
    }

    /// <summary>
    /// Invoke request object.
    /// </summary>
    public class InvokeRequest : BaseRequest
    {
        /// <summary>
        /// Path of the request.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Permission of the request.
        /// </summary>
        public readonly Permission Permission;

        /// <summary>
        /// Parameters of the request.
        /// </summary>
        public readonly JObject Parameters;

        /// <summary>
        /// Callback of the request.
        /// </summary>
        public readonly Action<InvokeResponse> Callback;

        /// <summary>
        /// Link container.
        /// </summary>
        private readonly DSLinkContainer _link;

        /// <summary>
        /// Columns of the request.
        /// </summary>
        private readonly JArray _columns;

        /// <summary>
        /// Whether this is the first update or not.
        /// </summary>
        private bool _firstUpdate = true;

        /// <summary>
        /// Mode of the table.
        /// </summary>
        public Table.Mode Mode;

        public InvokeRequest(int requestId, string path, Permission permission, JObject parameters,
            Action<InvokeResponse> callback = null, DSLinkContainer link = null,
            JArray columns = null) : base(requestId)
        {
            Path = path;
            Permission = permission;
            Parameters = parameters;
            Callback = callback;
            _link = link;
            _columns = columns;
        }

        /// <inheritdoc />
        public override string Method => "invoke";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override JObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized["path"] = Path;
            if (baseSerialized["permit"] != null)
            {
                baseSerialized["permit"] = Permission.ToString();
            }

            baseSerialized["params"] = Parameters;
            return baseSerialized;
        }

        /// <summary>
        /// Sends a table to the requester.
        /// </summary>
        /// <param name="table">Table</param>
        public async Task UpdateTable(Table table)
        {
            if (_link == null || _columns == null)
            {
                throw new NotSupportedException("Link or columns are null, cannot send updates");
            }

            var updateRootObject = new JObject
            {
                new JProperty("responses", new JArray
                {
                    new JObject
                    {
                        new JProperty("rid", RequestId),
                        new JProperty("stream", "open"),
                        new JProperty("updates", new JArray(table))
                    }
                })
            };
            if (_firstUpdate)
            {
                _firstUpdate = false;
                if (Mode != null)
                {
                    updateRootObject["responses"].First["meta"] = new JObject
                    {
                        new JProperty("mode", Mode.String)
                    };
                }

                updateRootObject["responses"].First["columns"] = _columns;
            }

            await _link.Connector.Write(updateRootObject);
        }

        /// <summary>
        /// Close the request.
        /// </summary>
        public async Task Close()
        {
            if (_link == null)
            {
                throw new NotSupportedException("Link is null, cannot send updates");
            }

            await _link.Connector.Write(new JObject
            {
                new JProperty("responses", new JArray
                {
                    new JObject
                    {
                        new JProperty("rid", RequestId),
                        new JProperty("stream", "closed")
                    }
                })
            });
        }
    }

    /// <summary>
    /// Subscribe request object.
    /// </summary>
    public class SubscribeRequest : BaseRequest
    {
        /// <summary>
        /// List of subscriptions paths.
        /// </summary>
        public readonly JArray Paths;

        /// <summary>
        /// Callback for value updates.
        /// </summary>
        public readonly Action<SubscriptionUpdate> Callback;

        public SubscribeRequest(int requestId, JArray paths, Action<SubscriptionUpdate> callback) : base(requestId)
        {
            Paths = paths;
            Callback = callback;
        }

        /// <inheritdoc />
        public override string Method => "subscribe";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override JObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized["paths"] = Paths;
            return baseSerialized;
        }
    }

    /// <summary>
    /// Unsubscribe request object.
    /// </summary>
    public class UnsubscribeRequest : BaseRequest
    {
        /// <summary>
        /// Subscription IDs.
        /// </summary>
        public readonly JArray Sids;

        public UnsubscribeRequest(int requestId, JArray sids) : base(requestId)
        {
            Sids = sids;
        }

        /// <inheritdoc />
        public override string Method => "unsubscribe";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override JObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized["sids"] = Sids;
            return baseSerialized;
        }
    }
}