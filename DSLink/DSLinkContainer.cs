using System;
using System.Threading.Tasks;
using DSLink.Connection;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Util.Logger;
using Newtonsoft.Json.Linq;
using DSLink.Platform;

namespace DSLink
{
    public class DSLinkContainer : AbstractContainer
    {
        private readonly Task _pingTask;
        internal SerializationManager SerializationManager;
        protected Handshake ProtocolHandshake;
        internal bool ReconnectOnFailure;
        private bool _isLinkInitialized;

        public DSLinkContainer(Configuration config) : base(config)
        {
            Logger = CreateLogger("DSLink");

            ReconnectOnFailure = true;
            Connector = BasePlatform.Current.CreateConnector(this);

            // Events
            Connector.OnMessage += OnStringRead;
            Connector.OnBinaryMessage += OnBinaryRead;
            Connector.OnWrite += OnStringWrite;
            Connector.OnBinaryWrite += OnBinaryWrite;
            Connector.OnOpen += OnOpen;
            Connector.OnClose += OnClose;

            // Overridable events for DSLink writers
            Connector.OnOpen += OnConnectionOpen;
            Connector.OnClose += OnConnectionClosed;

            _pingTask = Task.Factory.StartNew(OnPingTaskElapsed);
        }

        public DSLinkContainer(ConfigurationBuilder configBuilder) : this(configBuilder.Build())
        {}

        /// <summary>
        /// Initializes the DSLink's node structure by building, or
        /// loading from disk when the link has been ran before.
        /// </summary>
        public async Task Initialize()
        {
            if (_isLinkInitialized)
            {
                return;
            }
            _isLinkInitialized = true;

            if (Config.Responder)
            {
                var loaded = await LoadSavedNodes();
                if (!loaded)
                {
                    InitializeDefaultNodes();
                }
            }
        }

        /// <summary>
        /// Used to initialize the node structure when nodes.json does not
        /// exist yet or failed to load.
        /// </summary>
        public virtual void InitializeDefaultNodes()
        {}

        public async Task Connect()
        {
            await Initialize();

            ReconnectOnFailure = true;
            ProtocolHandshake = new Handshake(this);
            var attemptsLeft = Config.ConnectionAttemptLimit;
            var attempts = 1;
            while (attemptsLeft == -1 || attemptsLeft > 0)
            {
                var handshakeStatus = await ProtocolHandshake.Shake();
                if (handshakeStatus)
                {
                    SerializationManager = new SerializationManager(Config.CommunicationFormat);
                    Connector.Serializer = SerializationManager.Serializer;
                    await Connector.Connect();
                    return;
                }

                var delay = attempts;
                if (delay > Config.MaxConnectionCooldown)
                {
                    delay = Config.MaxConnectionCooldown;
                }
                Logger.Warning($"Failed to connect, delaying for {delay} seconds");
                await Task.Delay(TimeSpan.FromSeconds(delay));

                if (attemptsLeft > 0)
                {
                    attemptsLeft--;
                }
                attempts++;
            }
            Logger.Warning("Failed to connect within the allotted connection attempt limit.");
            OnConnectionFailed();
        }

        public void Disconnect()
        {
            ReconnectOnFailure = false;
            Connector.Disconnect();
        }

        public async Task<bool> LoadSavedNodes()
        {
            return await Responder.Deserialize();
        }

        public async Task SaveNodes()
        {
            await Responder.Serialize();
        }

        private void OnOpen()
        {
            Connector.Flush();
        }

        private async void OnClose()
        {
            Responder.SubscriptionManager.ClearAll();
            Responder.StreamManager.ClearAll();
            if (ReconnectOnFailure)
            {
                await Connect();
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
        /// Called when the connection fails to connect to the broker.
        /// Override when you need to detect a failure to connect.
        /// </summary>
        protected virtual void OnConnectionFailed() {}

        private async Task OnMessage(JObject message)
        {
            var response = new JObject();
            if (message["msg"] != null)
            {
                response["ack"] = message["msg"].Value<int>();
            }

            bool write = false;

            if (message["requests"] != null)
            {
                var responses = await Responder.ProcessRequests(message["requests"].Value<JArray>());
                if (responses.Count > 0)
                {
                    response["responses"] = responses;
                }
                write = true;
            }

            if (message["responses"] != null)
            {
                var requests = await Requester.ProcessResponses(message["responses"].Value<JArray>());
                if (requests.Count > 0)
                {
                    response["requests"] = requests;
                }
                write = true;
            }

            if (write)
            {
                await Connector.Write(response);
            }
        }

        private async void OnStringRead(MessageEvent messageEvent)
        {
            LogMessageString(false, messageEvent);
            await OnMessage(SerializationManager.Serializer.Deserialize(messageEvent.Message));
        }

        private void OnStringWrite(MessageEvent messageEvent)
        {
            LogMessageString(true, messageEvent);
        }

        private async void OnBinaryRead(BinaryMessageEvent messageEvent)
        {
            LogMessageBytes(false, messageEvent);
            await OnMessage(SerializationManager.Serializer.Deserialize(messageEvent.Message));
        }

        private void OnBinaryWrite(BinaryMessageEvent messageEvent)
        {
            LogMessageBytes(true, messageEvent);
        }

        private void LogMessageString(bool sent, MessageEvent messageEvent)
        {
            if (Logger.ToPrint.DoesPrint(LogLevel.Debug))
            {
                var verb = sent ? "Sent" : "Received";
                var logString = $"Text {verb}: {messageEvent.Message}";
                Logger.Debug(logString);
            }
        }

        private void LogMessageBytes(bool sent, BinaryMessageEvent messageEvent)
        {
            if (Logger.ToPrint.DoesPrint(LogLevel.Debug))
            {
                var verb = sent ? "Sent" : "Received";
                var logString = $"Binary {verb}: ";
                if (messageEvent.Message.Length < 5000)
                {
                    logString += BitConverter.ToString(messageEvent.Message);
                }
                else
                {
                    logString += "(over 5000 bytes)";
                }
                Logger.Debug(logString);
            }
        }

        private async void OnPingTaskElapsed()
        {
            while (_pingTask.Status != TaskStatus.Canceled)
            {
                if (Connector.Connected())
                {
                    // Write a blank message containing no responses/requests.
                    Logger.Debug("Sent ping");
                    await Connector.Write(new JObject(), false);
                }
                // TODO: Extract the amount of time to the configuration object.
                await Task.Delay(30000);
            }
        }
    }
}
