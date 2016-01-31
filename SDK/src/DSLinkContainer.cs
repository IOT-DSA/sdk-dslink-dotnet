using System;
using System.Timers;
using DSLink.Connection;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace DSLink
{
    /// <summary>
    /// DSLink implementation of a container.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class DSLinkContainer : AbstractContainer
    {
        /// <summary>
        /// Timer used to send pings across the communication layer
        /// </summary>
        private readonly Timer _pingTimer;

        /// <summary>
        /// SerializationManager handles serialization for communications
        /// </summary>
        internal readonly SerializationManager SerializationManager;

        /// <summary>
        /// Handshake object, which contains data from the initial connection handshake
        /// </summary>
        protected Handshake Handshake;

        /// <summary>
        /// DSLinkContainer constructor.
        /// </summary>
        /// <param name="config">Configuration for the DSLink</param>
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

        /// <summary>
        /// Event that fires when the connection is closed to the broker.
        /// </summary>
        private void OnClose()
        {
            Logger.Info("Connection Closed");
        }

        /// <summary>
        /// Event that fires when a plain text message is received from the broker.
        /// This deserializes the message and hands it off to OnMessage.
        /// </summary>
        /// <param name="messageEvent">Text message event</param>
        private void OnTextMessage(MessageEvent messageEvent)
        {
            Logger.Debug("Text Received: " + messageEvent.Message);
            OnMessage(SerializationManager.Serializer.Deserialize(messageEvent.Message));
        }

        /// <summary>
        /// Event that fires when a binary message is received from the broker.
        /// This deserializes the message and hands it off to OnMessage.
        /// </summary>
        /// <param name="messageEvent">Binary message event</param>
        private void OnBinaryMessage(BinaryMessageEvent messageEvent)
        {
            Logger.Debug("Binary Received: " + BitConverter.ToString(messageEvent.Message));
            OnMessage(SerializationManager.Serializer.Deserialize(messageEvent.Message));
        }

        /// <summary>
        /// Called when a message is received from the server, and is passed in deserialized data.
        /// </summary>
        /// <param name="message">Deserialized data</param>
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

        /// <summary>
        /// Event that is fired when a plain text message is sent to the broker.
        /// </summary>
        /// <param name="messageEvent">Text message event</param>
        private void OnWrite(MessageEvent messageEvent)
        {
            Logger.Debug("Text Sent: " + messageEvent.Message);
        }

        /// <summary>
        /// Event that is fired when a binary message is sent to the broker.
        /// </summary>
        /// <param name="messageEvent">Binary message event</param>
        private void OnBinaryWrite(BinaryMessageEvent messageEvent)
        {
            Logger.Debug("Binary Sent: " + BitConverter.ToString(messageEvent.Message));
        }

        /// <summary>
        /// Event that is fired when the ping timed elapses.
        /// </summary>
        /// <param name="sender">Unused</param>
        /// <param name="e">Unused</param>
        private void OnPingElapsed(object sender, ElapsedEventArgs e)
        {
            // Write a blank message containing no responses/requests.
            Connector.Write(new RootObject());
        }
    }
}
