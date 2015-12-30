using System;
using System.Timers;
using DSLink.Connection;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace DSLink
{
    // ReSharper disable once InconsistentNaming
    public class DSLinkContainer : AbstractContainer
    {
        private readonly Timer _pingTimer;
        internal readonly SerializationManager SerializationManager;

        protected Handshake Handshake;

        public DSLinkContainer(Configuration config) : base(config)
        {
            CreateLogger("DSLink");

            _pingTimer = new Timer(30 * 1000);
            _pingTimer.Elapsed += OnPingElapsed;

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
            var response = new RootObject
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
    }
}
