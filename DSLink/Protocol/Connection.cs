using System;
using System.Text;
using System.Threading.Tasks;
using DSLink.Logging;
using DSLink.Serializer;
using DSLink.Util;
using Newtonsoft.Json.Linq;

namespace DSLink.Protocol
{
    public abstract class Connection
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private BaseSerializer _serializer;
        private readonly Configuration _config;
        private readonly IncrementingIndex _msgId;

        public BaseSerializer DataSerializer
        {
            get
            {
                if (_serializer == null && _config != null)
                {
                    _serializer = (BaseSerializer) Activator.CreateInstance(
                        Serializers.Types[_config.CommunicationFormatUsed]
                    );
                }

                return _serializer;
            }
        }

        public ConnectionState ConnectionState
        {
            private set;
            get;
        }

        /// <summary>
        /// Queue object for queueing up data when the WebSocket is either closed
        /// or we want to send a large amount of data in one burst. When set to
        /// false, the flush method is automatically called.
        /// </summary>
        private JObject _queue;

        /// <summary>
        /// Queue lock object.
        /// </summary>
        private readonly object _queueLock = new object();

        /// <summary>
        /// Whether we should enable the queueing of messages.
        /// </summary>
        private bool _enableQueue = true;

        /// <summary>
        /// Do we have a queue flush event scheduled?
        /// </summary>
        private bool _hasQueueEvent;

        /// <summary>
        /// Subscription value queue.
        /// </summary>
        private JArray _subscriptionValueQueue = new JArray();

        /// <summary>
        /// Whether we should enable the queueing of messages.
        /// </summary>
        public bool EnableQueue
        {
            set
            {
                _enableQueue = value;
                if (!value)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Flush();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            get
            {
                //return _enableQueue;
                return false;
            }
        }

        /// <summary>
        /// True if the WebSocket implementation supports binary.
        /// </summary>
        public virtual bool SupportsBinary => false;

        /// <summary>
        /// True if the WebSocket implementation supports compression.
        /// </summary>
        public virtual bool SupportsCompression => false;

        /// <summary>
        /// Event occurs when String data is written over the connection.
        /// </summary>
        public event Action<MessageEvent> OnWrite;

        /// <summary>
        /// Event occurs when Binary data is written over the connection.
        /// </summary>
        public event Action<BinaryMessageEvent> OnBinaryWrite;

        /// <summary>
        /// Event occurs when the connection is opened.
        /// </summary>
        public event Action OnOpen;

        /// <summary>
        /// Event occurs when the connection is closed.
        /// </summary>
        public event Action OnClose;

        /// <summary>
        /// Event occurs when String data is received.
        /// </summary>
        public event Action<MessageEvent> OnMessage;

        /// <summary>
        /// Event occurs when Binary data is received.
        /// </summary>
        public event Action<BinaryMessageEvent> OnBinaryMessage;

        protected Connection(Configuration config)
        {
            _config = config;
            ConnectionState = ConnectionState.Disconnected;
            _msgId = new IncrementingIndex();

            OnOpen += () =>
            {
                ConnectionState = ConnectionState.Connected;
                Logger.Info($"Connected");
            };

            OnClose += () =>
            {
                ConnectionState = ConnectionState.Disconnected;
                Logger.Info("Disconnected");
            };
        }

        /// <summary>
        /// Rewrites the broker endpoint into the websocket connection endpoint.
        /// </summary>
        protected string WsUrl
        {
            get
            {
                var uri = new Uri(_config.BrokerUrl);
                var sb = new StringBuilder();

                sb.Append(uri.Scheme.Equals("https") ? "wss://" : "ws://");
                sb.Append(uri.Host).Append(":").Append(uri.Port).Append(_config.RemoteEndpoint.wsUri);
                sb.Append("?");
                sb.Append("dsId=").Append(_config.DsId);
                sb.Append("&auth=").Append(_config.Authentication);
                sb.Append("&format=").Append(_config.CommunicationFormatUsed);
                if (_config.HasToken)
                {
                    sb.Append("&token=").Append(_config.TokenParameter);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Connect to the broker.
        /// </summary>
        public virtual Task Connect()
        {
            ConnectionState = ConnectionState.Connecting;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnect from the broker.
        /// </summary>
        public virtual Task Disconnect()
        {
            ConnectionState = ConnectionState.Disconnecting;
            Logger.Info("Disconnecting");

            return Task.CompletedTask;
        }

        /// <summary>
        /// True if connected to a broker.
        /// </summary>
        public abstract bool Connected();

        /// <summary>
        /// Write the specified data.
        /// </summary>
        /// <param name="data">RootObject to serialize and send</param>
        /// <param name="allowQueue">Whether to allow the data to be added to the queue</param>
        public virtual async Task Write(JObject data, bool allowQueue = true)
        {
            if ((!Connected() || EnableQueue) && allowQueue)
            {
                lock (_queueLock)
                {
                    if (_queue == null)
                    {
                        _queue = new JObject
                        {
                            new JProperty("msg", _msgId.CurrentAndIncrement),
                            new JProperty("responses", new JArray()),
                            new JProperty("requests", new JArray())
                        };
                    }

                    if (data["responses"] != null)
                    {
                        foreach (var resp in data["responses"].Value<JArray>())
                        {
                            ((JArray) _queue["responses"]).Add(resp);
                        }
                    }

                    if (data["requests"] != null)
                    {
                        foreach (var req in data["requests"].Value<JArray>())
                        {
                            ((JArray) _queue["requests"]).Add(req);
                        }
                    }

                    if (data["ack"] != null)
                    {
                        _queue["ack"] = data["ack"];
                    }

                    if (!_hasQueueEvent)
                    {
                        // Set flag to queue flush
                        _hasQueueEvent = true;
                    }
                }

                if (_hasQueueEvent)
                {
                    await TriggerQueueFlush();
                }
            }

            if (data["msg"] == null)
            {
                data["msg"] = _msgId.CurrentAndIncrement;
            }

            if (data["requests"] != null && data["requests"].Value<JArray>().Count == 0)
            {
                data.Remove("requests");
            }

            if (data["responses"] != null && data["responses"].Value<JArray>().Count == 0)
            {
                data.Remove("responses");
            }

            try
            {
                await WriteData(DataSerializer.Serialize(data));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to send message, reconnecting.");
                await Disconnect();
            }
        }

        /// <summary>
        /// Called to add value updates.
        /// </summary>
        /// <param name="update">Value Update</param>
        public virtual async Task AddValueUpdateResponse(JToken update)
        {
            if (EnableQueue)
            {
                lock (_queueLock)
                {
                    _subscriptionValueQueue.Add(update);

                    if (!_hasQueueEvent)
                    {
                        _hasQueueEvent = true;
                        // Schedule event for queue flush.
                        Task.Run((() => TriggerQueueFlush()));
                    }
                }
            }
            else
            {
                var response = new JObject
                {
                    {"rid", 0},
                    {"updates", new JArray {update}}
                };

                await Write(new JObject
                {
                    {"responses", new JArray {response}}
                });
            }
        }

        /// <summary>
        /// Writes a string to the connector.
        /// </summary>
        /// <param name="data">String to write</param>
        public virtual Task Write(string data)
        {
            OnWrite?.Invoke(new MessageEvent(data));
            return Task.FromResult(false);
        }

        /// <summary>
        /// Writes binary to the connector.
        /// </summary>
        /// <param name="data">Binary to write</param>
        public virtual Task Write(byte[] data)
        {
            OnBinaryWrite?.Invoke(new BinaryMessageEvent(data));
            return Task.FromResult(false);
        }

        /// <summary>
        /// Emit the open connector event.
        /// </summary>
        protected virtual void EmitOpen()
        {
            OnOpen?.Invoke();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Flush();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Emit the close connector event.
        /// </summary>
        protected virtual void EmitClose()
        {
            OnClose?.Invoke();
        }

        /// <summary>
        /// Emit the message connector event.
        /// </summary>
        /// <param name="messageEvent">Message event</param>
        protected void EmitMessage(MessageEvent messageEvent)
        {
            OnMessage?.Invoke(messageEvent);
        }

        /// <summary>
        /// Emits the binary message.
        /// </summary>
        /// <returns>The binary message.</returns>
        /// <param name="messageEvent">Message event.</param>
        protected void EmitBinaryMessage(BinaryMessageEvent messageEvent)
        {
            OnBinaryMessage?.Invoke(messageEvent);
        }

        /// <summary>
        /// Flush the queue.
        /// </summary>
        internal async Task Flush(bool fromEvent = false)
        {
            if (!Connected())
            {
                return;
            }

            Logger.Debug("Flushing connection message queue");
            JObject _queueToFlush = null;
            lock (_queueLock)
            {
                if (fromEvent)
                {
                    _hasQueueEvent = false;
                }

                if (_subscriptionValueQueue.Count != 0)
                {
                    var response = new JObject
                    {
                        {"rid", 0},
                        {"updates", _subscriptionValueQueue}
                    };

                    if (_queue == null)
                    {
                        _queue = new JObject
                        {
                            {"responses", new JArray()}
                        };
                    }

                    _queue["responses"].Value<JArray>().Add(response);
                }

                if (_queue != null)
                {
                    _queueToFlush = _queue;
                    _queue = null;
                }

                if (_subscriptionValueQueue.Count != 0)
                {
                    _subscriptionValueQueue = new JArray();
                }
            }

            if (_queueToFlush != null)
            {
                await Write(_queueToFlush, false);
            }
        }

        /// <summary>
        /// Flushes the queue for scheduled queue events.
        /// </summary>
        internal async Task TriggerQueueFlush()
        {
            await Flush(true);
        }

        /// <summary>
        /// Write data to the connection.
        /// </summary>
        /// <param name="data">String or Binary data</param>
        private Task WriteData(dynamic data)
        {
            switch (data)
            {
                case string _:
                case byte[] _:
                    return Write(data);
                default:
                    throw new FormatException($"Cannot send message of type {data.Type}");
            }
        }
    }
}