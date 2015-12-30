using System;
using System.Text;
using DSLink.Connection.Serializer;
using WebSocketSharp;

namespace DSLink.Connection
{
    public class WebSocketConnector : Connector
    {
        private WebSocket _webSocket;

        public WebSocketConnector(Configuration config, ISerializer serializer) : base(config, serializer)
        {
        }

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

        public override void Connect()
        {
            _webSocket = new WebSocket(WsUrl);

            _webSocket.OnOpen += (sender, e) =>
            {
                EmitOpen();
            };

            _webSocket.OnClose += (sender, e) =>
            {
                EmitClose();
            };

            _webSocket.OnMessage += (sender, e) =>
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

        public override void Disconnect()
        {
            _webSocket.Close();
        }

        public override bool Connected()
        {
            return _webSocket.IsAlive;
        }

        protected override void WriteString(string data)
        {
            base.WriteString(data);
            _webSocket.Send(data);
        }

        protected override void WriteBinary(byte[] data)
        {
            base.WriteBinary(data);
            _webSocket.Send(data);
        }
    }
}
