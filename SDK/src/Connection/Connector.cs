using System;
using DSLink.Connection.Serializer;

namespace DSLink.Connection
{
    public abstract class Connector
    {
        protected readonly Configuration Config;
        private readonly ISerializer _serializer;

        public event Action<MessageEvent> OnWrite;
        public event Action<BinaryMessageEvent> OnBinaryWrite;
        public event Action OnOpen;
        public event Action OnClose;
        public event Action<MessageEvent> OnMessage;
        public event Action<BinaryMessageEvent> OnBinaryMessage;

        protected Connector(Configuration config, ISerializer serializer)
        {
            Config = config;
            _serializer = serializer;
        }

        public abstract void Connect();

        public abstract void Disconnect();

        public abstract bool Connected();

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

        protected virtual void WriteString(string data)
        {
            OnWrite?.Invoke(new MessageEvent(data));
        }

        protected virtual void WriteBinary(byte[] data)
        {
            OnBinaryWrite?.Invoke(new BinaryMessageEvent(data));
        }

        protected void EmitOpen()
        {
            OnOpen?.Invoke();
        }

        protected void EmitClose()
        {
            OnClose?.Invoke();
        }

        protected void EmitMessage(MessageEvent e)
        {
            OnMessage?.Invoke(e);
        }

        protected void EmitBinaryMessage(BinaryMessageEvent e)
        {
            OnBinaryMessage?.Invoke(e);
        }
    }

}
