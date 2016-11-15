using System;
using System.Text;
using System.Threading.Tasks;
using DSLink.Connection.Serializer;
using DSLink.Container;
using Newtonsoft.Json.Linq;

namespace DSLink.Connection
{
    public abstract class Connector
    {
        /// <summary>
        /// Instance of link container.
        /// </summary>
        protected readonly AbstractContainer _link;

        /// <summary>
        /// Gets the state of the WebSocket connection.
        /// </summary>
        public ConnectionState ConnectionState
        {
            private set;
            get;
        }

        /// <summary>
        /// Instance of serializer, used to serialize data going through the
        /// connection.
        /// </summary>
        public ISerializer Serializer
        {
            get;
            internal set;
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
                    Flush();
                }
            }
            get
            {
                return _enableQueue;
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

        /// <summary>
        /// Base constructor for connectors, registers default events for
        /// setting the connection state.
        /// </summary>
        /// <param name="link">Link</param>
        protected Connector(AbstractContainer link)
        {
            _link = link;
            ConnectionState = ConnectionState.Disconnected;

            OnOpen += () =>
            {
                ConnectionState = ConnectionState.Connected;
                _link.Logger.Info("Connected");
            };

            OnClose += () =>
            {
                ConnectionState = ConnectionState.Disconnected;
                _link.Logger.Info("Disconnected");
            };
        }

        /// <summary>
        /// Rewrites the broker endpoint into the websocket connection endpoint.
        /// </summary>
        protected string WsUrl
        {
            get
            {
                var config = _link.Config;
                var uri = new Uri(config.BrokerUrl);
                var sb = new StringBuilder();

                sb.Append(uri.Scheme.Equals("https") ? "wss://" : "ws://");
                sb.Append(uri.Host).Append(":").Append(uri.Port).Append(config.RemoteEndpoint.wsUri);
                sb.Append("?");
                sb.Append("dsId=").Append(config.DsId);
                sb.Append("&auth=").Append(config.Authentication);
                sb.Append("&format=").Append(config.CommunicationFormat);

                return sb.ToString();
            }
        }

        /// <summary>
        /// Connect to the broker.
        /// </summary>
        public virtual async Task Connect()
        {
            ConnectionState = ConnectionState.Connecting;
            _link.Logger.Info("Connecting");
        }

        /// <summary>
        /// Disconnect from the broker.
        /// </summary>
        public virtual void Disconnect()
        {
            ConnectionState = ConnectionState.Disconnecting;
            _link.Logger.Info("Disconnecting");
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
        public async Task Write(JObject data, bool allowQueue = true)
        {
            if ((!Connected() || EnableQueue) && allowQueue)
            {
                lock (_queueLock)
                {
                    if (_queue == null)
                    {
                        _queue = new JObject
                        {
                            new JProperty("msg", _link.MessageId),
                            new JProperty("responses", new JArray()),
                            new JProperty("requests", new JArray())
                        };
                    }

                    if (data["responses"] != null)
                    {
                        var responseQueue = _queue["responses"].Value<JArray>();
                        foreach (var resp in data["responses"].Value<JArray>())
                        {
                            responseQueue.Add(resp);
                        }
                    }

                    if (data["requests"] != null)
                    {
                        var requestQueue = _queue["requests"].Value<JArray>();
                        foreach (var req in data["requests"].Value<JArray>())
                        {
                            requestQueue.Add(req);
                        }
                    }

                    if (data["ack"] != null)
                    {
                        _queue["ack"] = data["ack"];
                    }

                    if (!_hasQueueEvent)
                    {
                        _hasQueueEvent = true;
                        // Schedule event for queue flush.
                        Task.Run((() => TriggerQueueFlush()));
                    }
                }
                return;
            }

            if (data["msg"] == null)
            {
                data["msg"] = _link.MessageId;
            }

            if (data["requests"] != null && data["requests"].Value<JArray>().Count == 0)
            {
                data.Remove("requests");
            }

            if (data["responses"] != null && data["responses"].Value<JArray>().Count == 0)
            {
                data.Remove("responses");
            }

            WriteData(Serializer.Serialize(data));
        }

        /// <summary>
        /// Called to add value updates.
        /// </summary>
        /// <param name="update">Value Update</param>
        public async Task AddValueUpdateResponse(JToken update)
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
                    {"updates", new JArray { update }}
                };

                await Write(new JObject
                {
                    {"responses", new JArray { response }}
                });
            }
        }

        /// <summary>
        /// Writes a string to the connector.
        /// </summary>
        /// <param name="data">String to write</param>
        public virtual void WriteString(string data)
        {
            OnWrite?.Invoke(new MessageEvent(data));
        }

        /// <summary>
        /// Writes binary to the connector.
        /// </summary>
        /// <param name="data">Binary to write</param>
        public virtual void WriteBinary(byte[] data)
        {
            OnBinaryWrite?.Invoke(new BinaryMessageEvent(data));
        }

        /// <summary>
        /// Emit the open connector event.
        /// </summary>
        protected virtual void EmitOpen()
        {
            OnOpen?.Invoke();
            Flush();
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
        internal void Flush(bool fromEvent = false)
        {
            if (!Connected())
            {
                return;
            }
            _link.Logger.Debug("Flushing connection message queue");
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
                    Write(_queue, false).Wait();
                    _queue = null;
                }

                if (_subscriptionValueQueue.Count != 0)
                {
                    _subscriptionValueQueue = new JArray();
                }
            }
        }

        /// <summary>
        /// Flushes the queue for scheduled queue events.
        /// </summary>
        internal void TriggerQueueFlush()
        {
            Flush(true);
        }

        /// <summary>
        /// Write data to the connection.
        /// </summary>
        /// <param name="data">String or Binary data</param>
        private void WriteData(dynamic data)
        {
            if (data is string)
            {
                WriteString(data);
            }
            else if (data is byte[])
            {
                WriteBinary(data);
            }
        }
    }
}
