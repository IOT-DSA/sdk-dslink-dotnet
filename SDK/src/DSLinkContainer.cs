using System;
using System.Timers;
using DSLink.Connection;
using DSLink.Connection.Serializer;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;

namespace DSLink
{
    // ReSharper disable once InconsistentNaming
    public class DSLinkContainer
    {
        public readonly ILog Logger = LogManager.GetLogger("DSLink");

        public readonly Configuration Config;
        private readonly Timer _pingTimer;
        internal readonly SerializationManager SerializationManager;

        protected Handshake Handshake;
        internal Connector Connector;
        private readonly Responder _responder;
        private readonly Requester _requester;
        private int _msg;
        private int _rid;
        public int MessageId => _msg++;
        public int RequestId => ++_rid;

        public DSLinkContainer(Configuration config)
        {
            ConfigureLogger();

            Config = config;
            _pingTimer = new Timer(30 * 1000);
            _pingTimer.Elapsed += OnPingElapsed;

            if (Config.Responder)
            {
                _responder = new Responder(this);
            }
            if (Config.Requester)
            {
                _requester = new Requester(this);
            }

            Handshake = new Handshake(this);
            Handshake.Shake();
            SerializationManager = new SerializationManager(config.CommunicationFormat);
            
            Connector = new WebSocketConnector(Config, SerializationManager.Serializer);
            Connector.OnMessage += OnTextMessage;
            Connector.OnBinaryMessage += OnBinaryMessage;
            Connector.OnWrite += OnWrite;
            Connector.OnBinaryWrite += OnBinaryWrite;
            Connector.OnClose += OnClose;
            Connector.Connect();
            OnPingElapsed(null, null);
            _pingTimer.Start();
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

        private void OnClose()
        {
            Logger.Info("Connection Closed");
        }

        private void OnTextMessage(MessageEvent messageEvent)
        {
            Logger.Debug("Text Received: " + messageEvent.Message);
            OnMessage(SerializationManager.Serializer.Deserialize(messageEvent.Message));
        }

        private void OnBinaryMessage(BinaryMessageEvent messageEvent)
        {
            Logger.Debug("Binary Received: " + BitConverter.ToString(messageEvent.Message));
            OnMessage(SerializationManager.Serializer.Deserialize(messageEvent.Message));
        }

        private void OnMessage(RootObject message)
        {
            RootObject response = new RootObject
            {
                Ack = message.Msg,
                Msg = MessageId
            };
            if (message.Requests != null)
            {
                response.Responses = Responder.ProcessRequests(message.Requests);
            }
            if (message.Responses != null)
            {
                // TODO
            }
            Connector.Write(response);
        }

        private void OnWrite(MessageEvent messageEvent)
        {
            Logger.Debug("Text Sent: " + messageEvent.Message);
        }

        private void OnBinaryWrite(BinaryMessageEvent messageEvent)
        {
            Logger.Debug("Binary Sent: " + BitConverter.ToString(messageEvent.Message));
        }

        private void OnPingElapsed(object sender, ElapsedEventArgs e)
        {
            Connector.Write(new RootObject());
        }

        private static void ConfigureLogger()
        {
            LayoutSkeleton layout = new PatternLayout("%date|%logger|%level|%message%newline");
            layout.ActivateOptions();

            IAppender appender = new ConsoleAppender
            {
                Threshold = Level.Debug,
                Layout = layout
            };

            BasicConfigurator.Configure(appender);
        }
    }
}
