using DSLink.Connection;
using DSLink.Request;
using DSLink.Respond;
using DSLink.Util.Logger;
using System.Threading.Tasks;

namespace DSLink.Container
{
    public abstract class AbstractContainer
    {
        private int _msg;
        private int _requestId;
        public int MessageId => _msg++;
        public int NextRequestId => ++_requestId;

        public abstract Configuration Config
        {
            get;
        }

        public abstract BaseLogger Logger
        {
            get;
        }

        public abstract Responder Responder
        {
            get;
        }

        public abstract Requester Requester
        {
            get;
        }

        public abstract Connector Connector
        {
            get;
        }

        public abstract Task<ConnectionState> Connect(uint maxAttempts = 0);
        public abstract void Disconnect();
    }
}
