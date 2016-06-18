using System;
using System.Threading.Tasks;
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
        /// Task used to send pings across the communication layer.
        /// </summary>
        private readonly Task _pingTask;

        /// <summary>
        /// SerializationManager handles serialization for communications.
        /// </summary>
        internal SerializationManager SerializationManager;

        /// <summary>
        /// Handshake object, which contains data from the initial connection handshake.
        /// </summary>
        protected Handshake Handshake;

        /// <summary>
        /// DSLinkContainer constructor.
        /// </summary>
        /// <param name="config">Configuration for the DSLink</param>
        public DSLinkContainer(Configuration config) : base(config)
        {
            CreateLogger("DSLink");

            DoHandshake();
            DoConnect();

            _pingTask = Task.Factory.StartNew(OnPingElapsed);
        }

        /// <summary>
        /// Performs handshake with broker.
        /// </summary>
        public void DoHandshake()
        {
            Handshake = new Handshake(this);
            Handshake.Shake();
            SerializationManager = new SerializationManager(Config.CommunicationFormat);
        }

        /// <summary>
        /// Connects to the Connector.
        /// </summary>
        public void DoConnect()
        {
            Connector = ConnectorManager.Create(this, Config, SerializationManager.Serializer);
            Connector.OnMessage += OnTextMessage;
            Connector.OnBinaryMessage += OnBinaryMessage;
            Connector.OnWrite += OnWrite;
            Connector.OnBinaryWrite += OnBinaryWrite;
            Connector.OnClose += OnClose;
            Connector.Connect();
        }

        /// <summary>
        /// Event that fires when the connection is closed to the broker.
        /// </summary>
        private void OnClose()
        {
            DoHandshake();
            DoConnect();
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
                response.Requests = Requester.ProcessRequests(message.Responses);
            }
            bool write = false;
            if (response.Requests != null && response.Requests.Count > 0)
            {
                write = true;
            }
            else if (response.Responses != null && response.Responses.Count > 0)
            {
                write = true;
            }
            if (write)
            {
                Connector.Write(response);
            }
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
        /// Task used to send pings occasionally to keep the connection alive.
        /// </summary>
        private void OnPingElapsed()
        {
            while (_pingTask.Status != TaskStatus.Canceled)
            {
                if (Connector.Connected())
                {
                    // Write a blank message containing no responses/requests.
                    Logger.Debug("Sent ping");
                    Connector.Write(new RootObject());
                }
                // TODO: Extract the amount of time to the configuration object.
                Task.Delay(30000).Wait();
            }
        }
    }
}
