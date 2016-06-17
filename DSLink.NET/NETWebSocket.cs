using System;
using System.Text;
using DSLink.Connection;
using DSLink.Connection.Serializer;
using DSLink.Container;
using WebSocketSharp;

namespace DSLink.NET
{
    public class NETWebSocketConnector : Connector
    {
        /// <summary>
        /// WebSocket client instance.
        /// </summary>
        private WebSocket _webSocket;

        public NETWebSocketConnector(AbstractContainer link, Configuration config, ISerializer serializer) : base(link, config, serializer)
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

            _webSocket = new WebSocket(WsUrl);

            _webSocket.OnOpen += (object sender, EventArgs e) =>
            {
                EmitOpen();
            };
            _webSocket.OnClose += (object sender, CloseEventArgs e) =>
            {
                if (e.WasClean)
                {
                    _link.Logger.Info(string.Format("WebSocket was closed cleanly with code {0}, and reason \"{1}\"", e.Code, e.Reason));
                }
                else
                {
                    _link.Logger.Info(string.Format("WebSocket was closed uncleanly with code {0}, and reason \"{1}\"", e.Code, e.Reason));
                }

                EmitClose();
            };

            _webSocket.OnError += (object sender, ErrorEventArgs e) =>
            {
                _link.Logger.Error(string.Format("WebSocket error: {0}", e.Message));
            };

            _webSocket.OnMessage += (object sender, MessageEventArgs e) =>
            {
                if (e.IsText)
                {
                    EmitMessage(new MessageEvent(e.Data));
                }
                else if (e.IsBinary)
                {
                    EmitBinaryMessage(new BinaryMessageEvent(e.RawData));
                }
            };

            _webSocket.Connect();
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
            return _webSocket.IsAlive;
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
            base.WriteBinary(data);
            _webSocket.Send(data);
        }
    }
}
