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

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Connection.MessageEvent"/> class.
        /// </summary>
        /// <param name="message">Message</param>
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

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Connection.BinaryMessageEvent"/> class.
        /// </summary>
        /// <param name="message">Message</param>
        public BinaryMessageEvent(byte[] message)
        {
            Message = message;
        }
    }
}
