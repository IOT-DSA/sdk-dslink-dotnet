using System;
using DSLink.Connection;
using DSLink.Util.Logger;

namespace DSLink.Container
{
    public abstract class AbstractContainer
    {
        public Configuration Config { get; protected set; }
        public Logger Logger { get; private set; }
        public Connector Connector { get; protected set; }
        private readonly Responder _responder;
        private readonly Requester _requester;
        private int _msg;
        private int _rid;
        public int MessageId => _msg++;
        public int RequestId => ++_rid;

        protected AbstractContainer(Configuration config)
        {
            Config = config;

            if (Config.Responder)
            {
                _responder = new Responder(this);
            }
            if (Config.Requester)
            {
                _requester = new Requester(this);
            }
        }

        public Responder Responder
        {
            get
            {
                if (!Config.Responder)
                {
                    throw new ArgumentException("Responder is not enabled.");
                }
                return _responder;
            }
        }

        public Requester Requester
        {
            get
            {
                if (!Config.Requester)
                {
                    throw new ArgumentException("Requester is not enabled.");
                }
                return _requester;
            }
        }

        protected void CreateLogger(string loggerName)
        {
            Logger = new Logger(loggerName);
        }
    }
}
