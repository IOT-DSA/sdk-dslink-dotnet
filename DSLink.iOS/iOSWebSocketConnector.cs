using System;
using System.Threading.Tasks;
using DSLink.Connection;
using Foundation;
using Square.SocketRocket;

namespace DSLink.iOS
{
    public class iOSWebSocketConnector : Connector
    {
        /// <summary>
        /// WebSocket client instance.
        /// </summary>
        private WebSocket _webSocket;

        public override bool SupportsBinary => true;

        public iOSWebSocketConnector(DSLinkContainer link) : base(link)
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
                _link.Logger.Info(e.WasClean
                    ? $"WebSocket was closed cleanly with code {e.Code}, and reason \"{e.Reason}\""
                    : $"WebSocket was closed uncleanly with code {e.Code}, and reason \"{e.Reason}\"");

                EmitClose();
            };

            _webSocket.WebSocketFailed += (sender, e) =>
            {
                _link.Logger.Error($"WebSocket error: {e.Error}");
            };

            _webSocket.ReceivedMessage += (sender, e) =>
            {
                var data = e.Message as NSData;
                if (data != null)
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
            _link.Logger.Debug("Sent binary " + BitConverter.ToString(data));
            _webSocket.Send(NSData.FromArray(data));
        }
    }
}
