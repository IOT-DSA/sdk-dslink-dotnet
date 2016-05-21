using System;
using System.Text;
using DSLink.Connection;
using DSLink.Connection.Serializer;
using Websockets;

namespace DSLink.Connection
{
    public class WebSocketBaseConnector : Connector
    {
        private IWebSocketConnection _webSocket;

        public WebSocketBaseConnector(Configuration config, ISerializer serializer) : base(config, serializer)
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
            _webSocket = WebSocketFactory.Create();

            _webSocket.OnOpened += EmitOpen;
            _webSocket.OnClosed += EmitClose;
            _webSocket.OnMessage += text =>
            {
                EmitMessage(new MessageEvent(text));
            };

            _webSocket.Open(WsUrl);
        }

        public override void Disconnect()
        {
            _webSocket.Close();
        }

        public override bool Connected()
        {
            return _webSocket.IsOpen;
        }

        protected override void WriteString(string data)
        {
            base.WriteString(data);
            _webSocket.Send(data);
        }

        protected override void WriteBinary(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
