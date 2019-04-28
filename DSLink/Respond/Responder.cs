using DSLink.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSLink.Respond
{
    public abstract class Responder
    {
        internal virtual BaseLinkHandler Link
        {
            get;
            set;
        }

        public virtual Node SuperRoot
        {
            get;
            protected set;
        }

        public virtual SubscriptionManager SubscriptionManager
        {
            get;
            protected set;
        }

        public virtual StreamManager StreamManager
        {
            get;
            protected set;
        }

        public virtual DiskSerializer DiskSerializer
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initialize the responder.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Process requests incoming from the broker.
        /// </summary>
        /// <param name="requests">List of requests</param>
        /// <returns>Responses to requester</returns>
        public abstract Task<JArray> ProcessRequests(JArray requests);
    }
}