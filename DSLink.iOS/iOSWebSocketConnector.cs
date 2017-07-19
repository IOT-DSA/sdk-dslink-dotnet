using System;
using System.Threading.Tasks;
using DSLink.Connection;
using Foundation;
using Square.SocketRocket;
using DSLink.Util.Logger;

namespace DSLink.iOS
{
    public class iOSWebSocketConnector : Connector
    {
        /// <summary>
        /// WebSocket client instance.
        /// </summary>
        private WebSocket _webSocket;

        public override bool SupportsBinary => true;

        public iOSWebSocketConnector(Configuration config, BaseLogger logger)
            : base(config, logger)
        {
        }

        /// <summary>
        /// Connect to the WebSocket.
        /// </summary>
        public override async Task Connect()
        {
            await base.Connect();

            _webSocket = new WebSocket(new NSUrl(WsUrl));

            _webSocket.WebSocketOpened += (sender, e) => EmitOpen();

            _webSocket.WebSocketClosed += (sender, e) =>
            {
                var cleanString = e.WasClean
                    ? "cleanly"
                    : "uncleanly";
                _logger.Info(
                    $"WebSocket was closed {cleanString} with code {e.Code}, and reason \"{e.Reason}\""
                );

                EmitClose();
            };

            _webSocket.WebSocketFailed += (sender, e) =>
            {
                _logger.Error($"WebSocket error: {e.Error}");
            };

            _webSocket.ReceivedMessage += (sender, e) =>
            {
                if (e.Message is NSData data)
                {
                    EmitBinaryMessage(new BinaryMessageEvent(data.ToArray()));
                }
                else
                {
                    EmitMessage(new MessageEvent(e.Message.ToString()));
                }
            };

            _webSocket.Open();
        }

        /// <summary>
        /// Disconnect from the WebSocket.
        /// </summary>
        public override void Disconnect()
        {
            base.Disconnect();

            _webSocket.Close();
            _webSocket.Dispose();
        }

        /// <summary>
        /// Returns true if the WebSocket is connected.
        /// </summary>
        public override bool Connected()
        {
            return _webSocket != null && _webSocket.ReadyState == ReadyState.Open;
        }

        /// <summary>
        /// Writes a string over the WebSocket connection.
        /// </summary>
        /// <param name="data">String data</param>
        public override void WriteString(string data)
        {
            base.WriteString(data);
            _webSocket.Send((NSString)data);
        }

        /// <summary>
        /// Writes binary over the WebSocket connection.
        /// </summary>
        /// <remarks>Not implemented</remarks>
        /// <param name="data">Binary data</param>
        public override void WriteBinary(byte[] data)
        {
            _logger.Debug("Sent binary " + BitConverter.ToString(data));
            _webSocket.Send(NSData.FromArray(data));
        }
    }
}
