namespace DSLink.Connection
{
    public class MessageEvent
    {
        public readonly string Message;

        public MessageEvent(string message)
        {
            Message = message;
        }
    }

    public class BinaryMessageEvent
    {
        public readonly byte[] Message;

        public BinaryMessageEvent(byte[] message)
        {
            Message = message;
        }
    }
}
