namespace DSLink.Connection
{
    /// <summary>
    /// Connection message event for String data.
    /// </summary>
    public class MessageEvent
    {
        /// <summary>
        /// String message data.
        /// </summary>
        public readonly string Message;

        public MessageEvent(string message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Connection message event for Binary data.
    /// </summary>
    public class BinaryMessageEvent
    {
        /// <summary>
        /// Binary message data.
        /// </summary>
        public readonly byte[] Message;

        public BinaryMessageEvent(byte[] message)
        {
            Message = message;
        }
    }
}
