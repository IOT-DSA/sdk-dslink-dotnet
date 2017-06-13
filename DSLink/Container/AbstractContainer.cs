using System;
using DSLink.Connection;
using DSLink.Request;
using DSLink.Respond;
using DSLink.Util.Logger;
using DSLink.Platform;

namespace DSLink.Container
{
    /// <summary>
    /// Abstract container for DSLink.
    /// </summary>
    public abstract class AbstractContainer
    {
        public Configuration Config { get; protected set; }

        /// <summary>
        /// Logger for DSLink.
        /// </summary>
        public BaseLogger Logger { get; internal set; }

        /// <summary>
        /// Gets or sets the connector.
        /// </summary>
        public Connector Connector { get; protected set; }

        /// <summary>
        /// Responder.
        /// </summary>
        private readonly Responder _responder;

        /// <summary>
        /// Requester.
        /// </summary>
        private readonly Requester _requester;

        /// <summary>
        /// Next message identifier.
        /// </summary>
        private int _msg;

        /// <summary>
        /// Next request identifier.
        /// </summary>
        private int _rid;

        /// <summary>
        /// Get the next message identifier.
        /// </summary>
        /// <value>The message identifier.</value>
        public int MessageId => _msg++;

        /// <summary>
        /// Get the next request identifier.
        /// </summary>
        /// <value></value>
        public int RequestId => ++_rid;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Container.AbstractContainer"/> class.
        /// </summary>
        /// <param name="config">Link configuration</param>
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

        /// <summary>
        /// Instance of the responder.
        /// </summary>
        /// <value>The responder.</value>
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

        /// <summary>
        /// Instance of the requester.
        /// </summary>
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

        /// <summary>
        /// Create a logger for the DSLink.
        /// </summary>
        /// <param name="loggerName">Logger name.</param>
        protected BaseLogger CreateLogger(string loggerName)
        {
            return BasePlatform.Current.CreateLogger(loggerName, Config.LogLevel);
        }
    }
}
