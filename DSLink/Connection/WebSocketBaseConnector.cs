using System;
using System.Text;
using DSLink.Connection;
using DSLink.Connection.Serializer;
using DSLink.Container;
using Websockets;

namespace DSLink.Connection
{
    public class WebSocketBaseConnector : Connector
    {
        /// <summary>
        /// WebSocket client instance.
        /// </summary>
        private IWebSocketConnection _webSocket;

        public WebSocketBaseConnector(AbstractContainer link, Configuration config, ISerializer serializer) : base(link, config, serializer)
        {
        }

        /// <summary>
        /// Builds the WebSocket URL.
        /// </summary>
        private string WsUrl
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
        /// Connect to the WebSocket.
        /// </summary>
        public override void Connect()
        {
            base.Connect();

            _webSocket = WebSocketFactory.Create();

            _webSocket.OnOpened += EmitOpen;
            _webSocket.OnClosed += EmitClose;
            _webSocket.OnError += (string error) =>
            {
                _link.Logger.Error("WebSocket error: " + error);
            };
            _webSocket.OnMessage += text =>
            {
                EmitMessage(new MessageEvent(text));
            };

            _webSocket.Open(WsUrl);
        }

        /// <summary>
        /// Disconnect from the WebSocket.
        /// </summary>
        public override void Disconnect()
        {
            base.Disconnect();

            _webSocket.Close();
        }

        /// <summary>
        /// Returns true if the WebSocket is connected.
        /// </summary>
        public override bool Connected()
        {
            return _webSocket.IsOpen;
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
