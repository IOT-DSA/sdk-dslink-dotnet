using System;
using System.Collections.Generic;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Respond;

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
        public int RequestID
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DSLink.Request.BaseRequest"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        protected BaseRequest(int requestID)
        {
            RequestID = requestID;
        }

        /// <summary>
        /// Request method.
        /// </summary>
        public abstract string Method();

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public virtual RequestObject Serialize()
        {
            return new RequestObject()
            {
                RequestId = RequestID,
                Method = Method()
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

        /// <summary>
        /// Link container instance.
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Request.ListRequest"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="callback">Callback</param>
        /// <param name="path">Path</param>
        /// <param name="link">Link</param>
        public ListRequest(int requestID, Action<ListResponse> callback, string path, AbstractContainer link) : base(requestID)
        {
            Callback = callback;
            Path = path;
            _link = link;
        }

        /// <summary>
        /// Request method.
        /// </summary>
        public override string Method() => "list";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Path;
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

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Request.SetRequest"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="path">Path</param>
        /// <param name="permission">Permission</param>
        /// <param name="value">Value</param>
        public SetRequest(int requestID, string path, Permission permission, Value value) : base(requestID)
        {
            Path = path;
            Permission = permission;
            Value = value;
        }

        /// <summary>
        /// Method of this request.
        /// </summary>
        public override string Method() => "set";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Path;
            baseSerialized.Permit = Permission.ToString();
            baseSerialized.Value = Value.Get();
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
        public string Path
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Request.RemoveRequest"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="path">Path.</param>
        public RemoveRequest(int requestID, string path) : base(requestID)
        {
            Path = path;
        }

        /// <summary>
        /// Method of the request.
        /// </summary>
        public override string Method() => "remove";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Path;
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
        public readonly Dictionary<string, dynamic> Parameters;

        /// <summary>
        /// Callback of the request.
        /// </summary>
        public readonly Action<InvokeResponse> Callback;

        /// <summary>
        /// Link container.
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// Columns of the request.
        /// </summary>
        private readonly List<Column> _columns;

        /// <summary>
        /// Whether this is the first update or not.
        /// </summary>
        private bool _firstUpdate = true;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Request.InvokeRequest"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="path">Path</param>
        /// <param name="permission">Permission</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="callback">Callback</param>
        /// <param name="link">Link</param>
        /// <param name="columns">Columns</param>
        public InvokeRequest(int requestID, string path, Permission permission, Dictionary<string, dynamic> parameters,
                             Action<InvokeResponse> callback = null, AbstractContainer link = null, List<Column> columns = null) : base(requestID)
        {
            Path = path;
            Permission = permission;
            Parameters = parameters;
            Callback = callback;
            _link = link;
            _columns = columns;
        }

        /// <summary>
        /// Method of the request.
        /// </summary>
        public override string Method() => "invoke";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Path;
            if (baseSerialized.Permit != null)
            {
                baseSerialized.Permit = Permission.ToString();
            }
            baseSerialized.Parameters = Parameters;
            return baseSerialized;
        }

        /// <summary>
        /// Sends an update to the requester.
        /// </summary>
        /// <param name="updates">Updates</param>
        /// <param name="close">Whether to close the stream</param>
        public void SendUpdates(List<dynamic> updates, bool close = false)
        {
            if (_link == null || _columns == null)
            {
                throw new NotSupportedException("Link and columns are null, cannot send updates");
            }
            var updateRootObject = new RootObject
            {
                Responses = new List<ResponseObject>
                {
                    new ResponseObject
                    {
                        RequestId = RequestID,
                        Stream = (close ? "close" : "open"),
                        Updates = updates
                    }
                }
            };
            if (_firstUpdate)
            {
                _firstUpdate = false;
                updateRootObject.Responses[0].Meta = new Dictionary<string, dynamic>
                {
                    {"mode", "append"},
                    {"meta", new Dictionary<string, dynamic>()}
                };
                updateRootObject.Responses[0].Columns = _columns;
            }
            _link.Connector.Write(updateRootObject);
        }

        /// <summary>
        /// Close the request.
        /// </summary>
        public void Close()
        {
            if (_link == null)
            {
                throw new NotSupportedException("Link is null, cannot send updates");
            }
            _link.Connector.Write(new RootObject
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
    /// Subscribe request object.
    /// </summary>
    public class SubscribeRequest : BaseRequest
    {
        /// <summary>
        /// List of subscriptions paths.
        /// </summary>
        public readonly List<AddSubscriptionObject> Paths;

        /// <summary>
        /// Callback for value updates.
        /// </summary>
        public readonly Action<SubscriptionUpdate> Callback;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Request.SubscribeRequest"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="paths">Paths</param>
        /// <param name="callback">Callback</param>
        public SubscribeRequest(int requestID, List<AddSubscriptionObject> paths, Action<SubscriptionUpdate> callback) : base(requestID)
        {
            Paths = paths;
            Callback = callback;
        }

        /// <summary>
        /// Method of this request.
        /// </summary>
        public override string Method() => "subscribe";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Paths = Paths;
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
        public readonly List<int> Sids;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Request.UnsubscribeRequest"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="sids">Subscription IDs</param>
        public UnsubscribeRequest(int requestID, List<int> sids) : base(requestID)
        {
            Sids = sids;
        }

        /// <summary>
        /// Method of this request.
        /// </summary>
        public override string Method() => "unsubscribe";

        /// <summary>
        /// Serialize the request.
        /// </summary>
        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.SubscriptionIds = Sids;
            return baseSerialized;
        }
    }
}
