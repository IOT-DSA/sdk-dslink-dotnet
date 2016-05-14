using System;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace DSLink.Connection
{
    public abstract class Connector
    {
        protected readonly AbstractContainer _link;
        protected readonly Configuration Config;
        private readonly ISerializer _serializer;

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
        /// <param name="serializer">Serializer.</param>
        protected Connector(AbstractContainer link, Configuration config, ISerializer serializer)
        {
            _link = link;
            Config = config;
            _serializer = serializer;
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
        /// Returns true if connected to a broker.
        /// </summary>
        public abstract bool Connected();

        /// <summary>
        /// Write the specified data.
        /// </summary>
        /// <param name="data">Data.</param>
        public void Write(RootObject data)
        {
            var serialized = _serializer.Serialize(data);
            if (serialized is string)
            {
                WriteString(serialized);
            }
            else if (serialized is byte[])
            {
                WriteBinary(serialized);
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
    }

}
