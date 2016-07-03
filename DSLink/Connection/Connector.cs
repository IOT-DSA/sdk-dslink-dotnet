using System;
using System.Collections.Generic;
using System.Text;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace DSLink.Connection
{
    public abstract class Connector
    {
        protected readonly AbstractContainer _link;
        protected readonly Configuration Config;
        public ISerializer Serializer
        {
            get;
            internal set;
        }
        private Queue<dynamic> _queue;

        public event Action<MessageEvent> OnWrite;
        public event Action<BinaryMessageEvent> OnBinaryWrite;
        public event Action OnOpen;
        public event Action OnClose;
        public event Action<MessageEvent> OnMessage;
        public event Action<BinaryMessageEvent> OnBinaryMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DSLink.Connection.Connector"/> class.
        /// </summary>
        /// <param name="link">Link.</param>
        /// <param name="config">Config.</param>
        protected Connector(AbstractContainer link, Configuration config)
        {
            _link = link;
            Config = config;
            _queue = new Queue<dynamic>();
        }

        /// <summary>
        /// Builds the WebSocket URL.
        /// </summary>
        protected string WsUrl
        {
            get
            {
                var uri = new Uri(Config.BrokerUrl);
                var sb = new StringBuilder();

                sb.Append(uri.Scheme.Equals("https") ? "wss://" : "ws://");
                sb.Append(uri.Host).Append(":").Append(uri.Port).Append(Config.RemoteEndpoint.wsUri);
                sb.Append("?");
                sb.Append("dsId=").Append(Config.DsId);
                sb.Append("&auth=").Append(Config.Authentication);
                sb.Append("&format=").Append(Config.CommunicationFormat);

                return sb.ToString();
            }
        }
        /// <summary>
        /// Connect to the broker.
        /// </summary>
        public virtual void Connect()
        {
            _link.Logger.Info("Connecting");
        }

        /// <summary>
        /// Disconnect from the broker.
        /// </summary>
        public virtual void Disconnect()
        {
            _link.Logger.Info("Disconnecting");
        }

        /// <summary>
        /// True if connected to a broker.
        /// </summary>
        public abstract bool Connected();

        /// <summary>
        /// True if the WebSocket implementation supports compression.
        /// </summary>
        public virtual bool SupportsCompression() => false;

        /// <summary>
        /// Write the specified data.
        /// </summary>
        /// <param name="data">Data.</param>
        public void Write(RootObject data)
        {
            if (!data.Msg.HasValue)
            {
                data.Msg = _link.MessageId;
            }
            var serialized = Serializer.Serialize(data);

            if (Connected())
            {
                WriteData(serialized);
            }
            else
            {
                lock (_queue)
                {
                    _queue.Enqueue(serialized);
                }
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
        internal void Flush()
        {
            lock (_queue)
            {
                while (_queue.Count > 0)
                {
                    WriteData(_queue.Dequeue());
                }
            }
        }

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
