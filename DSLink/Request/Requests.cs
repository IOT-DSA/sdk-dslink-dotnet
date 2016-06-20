using System;
using System.Collections.Generic;
using DSLink.Connection.Serializer;
using DSLink.Nodes;
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
        /// Initializes a new instance of the <see cref="T:DSLink.Request.ListRequest"/> class.
        /// </summary>
        /// <param name="requestID">Request identifier</param>
        /// <param name="callback">Callback</param>
        /// <param name="path">Path</param>
        public ListRequest(int requestID, Action<ListResponse> callback, string path) : base(requestID)
        {
            Callback = callback;
            Path = path;
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

        public readonly Permission Permission;

        public readonly Value Value;

        public SetRequest(int requestID, string path, Permission permission, Value value) : base(requestID)
        {
            Path = path;
            Permission = permission;
            Value = value;
        }

        public override string Method() => "set";

        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Path;
            baseSerialized.Permit = Permission.ToString();
            baseSerialized.Value = Value.Get();
            return baseSerialized;
        }
    }

    public class RemoveRequest : BaseRequest
    {
        public string Path
        {
            get;
        }

        public RemoveRequest(int requestID, string path) : base(requestID)
        {
            Path = path;
        }

        public override string Method() => "remove";

        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Path;
            return baseSerialized;
        }
    }

    public class InvokeRequest : BaseRequest
    {
        public readonly string Path;

        public readonly Permission Permission;

        public readonly Dictionary<string, dynamic> Parameters;

        public readonly Action<InvokeResponse> Callback;

        public InvokeRequest(int requestID, string path, Permission permission, Dictionary<string, dynamic> parameters, Action<InvokeResponse> callback) : base(requestID)
        {
            Path = path;
            Permission = permission;
            Parameters = parameters;
            Callback = callback;
        }

        public override string Method() => "invoke";

        public override RequestObject Serialize()
        {
            var baseSerialized = base.Serialize();
            baseSerialized.Path = Path;
            baseSerialized.Permit = Permission.ToString();
            baseSerialized.Parameters = Parameters;
            return baseSerialized;
        }
    }
}
