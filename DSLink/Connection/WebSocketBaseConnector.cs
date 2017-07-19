using DSLink.Util.Logger;
using System;
using System.Threading.Tasks;
using Websockets;

namespace DSLink.Connection
{
    public class WebSocketBaseConnector : Connector
    {
        /// <summary>
        /// WebSocket client instance.
        /// </summary>
        private IWebSocketConnection _webSocket;

        public WebSocketBaseConnector(Configuration config, BaseLogger logger)
            : base(config, logger)
        {
        }

        /// <summary>
        /// Connect to the WebSocket.
        /// </summary>
        public override async Task Connect()
        {
            await base.Connect();

            _webSocket = WebSocketFactory.Create();

            _webSocket.OnOpened += EmitOpen;
            _webSocket.OnClosed += EmitClose;
            _webSocket.OnError += error =>
            {
                _logger.Error("WebSocket error: " + error);
            };
            _webSocket.OnMessage += text =>
            {
                EmitMessage(new MessageEvent(text));
            };

            _logger.Info("WebSocket connecting to " + WsUrl);
            _webSocket.Open(WsUrl);
        }

        /// <summary>
        /// Disconnect from the WebSocket.
        /// </summary>
        public override void Disconnect()
        {
            base.Disconnect();

            if (_webSocket != null)
            {
                _webSocket.Close();
                _webSocket.Dispose();
            }
        }

        /// <summary>
        /// Returns true if the WebSocket is connected.
        /// </summary>
        public override bool Connected()
        {
            return _webSocket != null && _webSocket.IsOpen;
        }

        /// <summary>
        /// Writes a string over the WebSocket connection.
        /// </summary>
        /// <param name="data">String data</param>
        public override void WriteString(string data)
        {
            base.WriteString(data);
            _webSocket.Send(data);
        }

        /// <summary>
        /// Writes binary over the WebSocket connection.
        /// </summary>
        /// <remarks>Not implemented</remarks>
        /// <param name="data">Binary data</param>
        public override void WriteBinary(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
