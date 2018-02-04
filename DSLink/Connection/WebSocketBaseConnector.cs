using DSLink.Util.Logger;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSLink.Connection
{
    public class WebSocketConnector : Connector
    {
        private readonly ClientWebSocket _ws;
        private readonly CancellationTokenSource _tokenSource;

        public WebSocketConnector(Configuration config, BaseLogger logger)
            : base(config, logger)
        {
            _ws = new ClientWebSocket();
            _tokenSource = new CancellationTokenSource();
        }

        public override async Task Connect()
        {
            await base.Connect();

            _logger.Info("WebSocket connecting to " + WsUrl);
            await _ws.ConnectAsync(new Uri(WsUrl), CancellationToken.None);
            _startWatchTask();
            EmitOpen();
        }

        /// <summary>
        /// Disconnect from the WebSocket.
        /// </summary>
        public async override Task Disconnect()
        {
            _stopWatchTask();
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", _tokenSource.Token);
            _ws.Dispose();

            await base.Disconnect();
        }

        /// <summary>
        /// Returns true if the WebSocket is connected.
        /// </summary>
        public override bool Connected()
        {
            return _ws != null && _ws.State == WebSocketState.Open;
        }

        /// <summary>
        /// Writes a string over the WebSocket connection.
        /// </summary>
        /// <param name="data">String data</param>
        public override Task Write(string data)
        {
            base.Write(data);
            var bytes = Encoding.UTF8.GetBytes(data);
            return _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _tokenSource.Token);
        }

        /// <summary>
        /// Writes binary over the WebSocket connection.
        /// </summary>
        /// <param name="data">Binary data</param>
        public override Task Write(byte[] data)
        {
            base.Write(data);
            return _ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, _tokenSource.Token);
        }

        private void _startWatchTask()
        {
            Task.Run(async () =>
            {
                var token = _tokenSource.Token;

                while (_ws.State == WebSocketState.Open)
                {
                    var buffer = new byte[1024];
                    var bufferUsed = 0;
                    var bytes = new List<byte>();
                    var str = "";

                    RECV:
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result == null)
                    {
                        goto RECV;
                    }

                    bufferUsed = result.Count;

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await Disconnect();
                            break;
                        case WebSocketMessageType.Text:
                            {
                                str += Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                                if (!result.EndOfMessage)
                                    goto RECV;
                                EmitMessage(new MessageEvent(str));
                            }
                            break;
                        case WebSocketMessageType.Binary:
                            {
                                var newBytes = new byte[bufferUsed];
                                Array.Copy(buffer, newBytes, bufferUsed);
                                bytes.AddRange(newBytes);
                                if (!result.EndOfMessage)
                                    goto RECV;
                                EmitBinaryMessage(new BinaryMessageEvent(bytes.ToArray()));
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (token.IsCancellationRequested)
                    {
                        await Disconnect();
                    }
                }

                return Task.CompletedTask;
            }, _tokenSource.Token);
        }

        private void _stopWatchTask()
        {
            _tokenSource.Cancel();
        }
    }
}
