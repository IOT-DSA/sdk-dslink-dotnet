using System;
using System.Collections.Generic;
using System.Text;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace DSLink.Connection
{
    public abstract class Connector
    {
        /// <summary>
        /// Instance of link container.
        /// </summary>
        protected readonly AbstractContainer _link;

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
        /// Queue used when the connection is in the closed state.
        /// </summary>
        private Queue<RootObject> _queue;

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
        /// Initializes a new instance of the <see cref="T:DSLink.Connection.Connector"/> class.
        /// </summary>
        /// <param name="link">Link.</param>
        protected Connector(AbstractContainer link)
        {
            _link = link;
            _queue = new Queue<RootObject>();
        }

        /// <summary>
        /// Builds the WebSocket URL.
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
        /// Write the specified data.
        /// </summary>
        /// <param name="data">Data.</param>
        public void Write(RootObject data)
        {
            if (!Connected())
            {
                lock (_queue)
                {
                    _queue.Enqueue(data);
                }
                return;
            }
            if (!data.Msg.HasValue)
            {
                data.Msg = _link.MessageId;
            }
            WriteData(Serializer.Serialize(data));
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
                    Write(_queue.Dequeue());
                }
            }
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
