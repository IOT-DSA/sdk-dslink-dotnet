using System;
using System.Threading.Tasks;
using DSLink.Connection;
using DSLink.Container;
using WebSocketSharp;

namespace DSLink.NET
{
    public class WebSocketSharpConnector : Connector
    {
        private WebSocket _webSocket;

        public override bool SupportsBinary => true;
        public override bool SupportsCompression => false;

        public WebSocketSharpConnector(AbstractContainer link) : base(link)
        {
        }

        public async override Task Connect()
        {
            await base.Connect();

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

            _webSocket.ConnectAsync();
        }

        public override void Disconnect()
        {
            base.Disconnect();

            _webSocket.Close();
        }

        public override bool Connected()
        {
            return _webSocket != null && _webSocket.IsAlive;
        }

        public override void WriteString(string data)
        {
            base.WriteString(data);
            _webSocket.Send(data);
        }

        public override void WriteBinary(byte[] data)
        {
            base.WriteBinary(data);
            _webSocket.Send(data);
        }
    }
}
