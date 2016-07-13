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
        /// Whether to reconnect to the broker. 
        /// </summary>
        internal bool Reconnect = true;

        /// <summary>
        /// DSLinkContainer constructor.
        /// </summary>
        /// <param name="config">Configuration for the DSLink</param>
        public DSLinkContainer(Configuration config) : base(config)
        {
            CreateLogger("DSLink");

            Reconnect = true;
            Connector = ConnectorManager.Create(this);

            // Events
            Connector.OnMessage += OnTextMessage;
            Connector.OnBinaryMessage += OnBinaryMessage;
            Connector.OnWrite += OnWrite;
            Connector.OnBinaryWrite += OnBinaryWrite;
            Connector.OnOpen += OnOpen;
            Connector.OnClose += OnClose;

            // Overridable events for DSLink writers
            Connector.OnOpen += OnConnectionOpen;
            Connector.OnClose += OnConnectionClosed;

            _pingTask = Task.Factory.StartNew(OnPingElapsed);
        }

        private void Delay(int attempts)
        {
            var delay = attempts * 1000;
            Logger.Warning(string.Format("Failed to connect, delaying for {0}ms", delay));
            Task.Delay(delay).Wait();
        }

        /// <summary>
        /// Connect to the broker.
        /// </summary>
        public void Connect()
        {
            Reconnect = true;
            Handshake = new Handshake(this);
            var attemptsLeft = Config.ConnectionAttemptLimit;
            var attempts = 1;
            while (attemptsLeft == -1 || attemptsLeft > 0)
            {
                if (Handshake.Shake())
                {
                    SerializationManager = new SerializationManager(Config.CommunicationFormat);
                    Connector.Serializer = SerializationManager.Serializer;
                    Connector.Connect();
                    if (Connector.Connected())
                    {
                        return;
                    }
                }

                Delay(attempts);

                if (attemptsLeft > 0)
                {
                    attemptsLeft--;
                }
                attempts++;
            }
            Logger.Warning("Failed to connect within the allotted connection attempt limit.");
        }

        /// <summary>
        /// Disconnect from the broker.
        /// </summary>
        public void Disconnect()
        {
            Reconnect = false;
            Connector.Disconnect();
        }

        /// <summary>
        /// Event that fires when the connection to the broker is complete.
        /// </summary>
        private void OnOpen()
        {
            Connector.Flush();
        }

        /// <summary>
        /// Event that fires when the connection is closed to the broker.
        /// </summary>
        private void OnClose()
        {
            Responder.SubscriptionManager.ClearAll();
            Responder.StreamManager.ClearAll();
            if (Reconnect)
            {
                Connect();
            }
        }

        /// <summary>
        /// Called when the connection is opened to the broker.
        /// Override when you need to do something after connection opens.
        /// </summary>
        protected virtual void OnConnectionOpen() {}

        /// <summary>
        /// Called when the connection is closed to the broker.
        /// Override when you need to do something after connection closes.
        /// </summary>
        protected virtual void OnConnectionClosed() {}

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
            if (messageEvent.Message.Length < 500)
            {
                Logger.Debug("Binary Received: " + BitConverter.ToString(messageEvent.Message));
            }
            else
            {
                Logger.Debug("Binary Received: (over 5000 bytes)");
            }

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
                Ack = message.Msg
            };
            bool write = false;
            if (message.Requests != null)
            {
                var responses = Responder.ProcessRequests(message.Requests);
                if (responses.Count > 0)
                {
                    response.Responses = responses;
                }
                write = true;
            }
            if (message.Responses != null)
            {
                var requests = Requester.ProcessResponses(message.Responses);
                if (requests.Count > 0)
                {
                    response.Requests = requests;
                }
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
            if (messageEvent.Message.Length < 5000)
            {
                Logger.Debug("Binary Sent: " + BitConverter.ToString(messageEvent.Message));
            }
            else
            {
                Logger.Debug("Binary Sent: (over 5000 bytes)");
            }
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
