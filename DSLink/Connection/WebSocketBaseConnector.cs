using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSLink.Logging;

namespace DSLink.Connection
{
    public class WebSocketConnector : Connector
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ClientWebSocket _ws;
        private readonly CancellationTokenSource _wsTokenSource;
        private readonly SemaphoreSlim _wsSendSemaphore;

        public WebSocketConnector(Configuration config) : base(config)
        {
            _ws = new ClientWebSocket();
            _wsTokenSource = new CancellationTokenSource();
            _wsSendSemaphore = new SemaphoreSlim(1, 1);
        }

        public override async Task Connect()
        {
            await base.Connect();

            Logger.Info("WebSocket connecting to " + WsUrl);
            await _ws.ConnectAsync(new Uri(WsUrl), CancellationToken.None);
            _startReceiveTask();
            EmitOpen();
        }

        /// <summary>
        /// Disconnect from the WebSocket.
        /// </summary>
        public override async Task Disconnect()
        {
            _stopWebSocketTasks();
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", _wsTokenSource.Token);
            _ws.Dispose();
            EmitClose();

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
            return _sendSegment(new ArraySegment<byte>(bytes), WebSocketMessageType.Text);
        }

        /// <summary>
        /// Writes binary over the WebSocket connection.
        /// </summary>
        /// <param name="data">Binary data</param>
        public override Task Write(byte[] data)
        {
            base.Write(data);
            return _sendSegment(new ArraySegment<byte>(data), WebSocketMessageType.Binary);
        }

        private async Task _sendSegment(ArraySegment<byte> segment, WebSocketMessageType msgType)
        {
            // Acquire sending lock.
            await _wsSendSemaphore.WaitAsync();
            try
            {
                // Attempt send.
                await _ws.SendAsync(segment, msgType, true, _wsTokenSource.Token);
            }
            catch
            {
                // Failed, log warning and disconnect.
                Logger.Warn("SendAsync call failed. Disconnecting from WebSocket.");
                await Disconnect();
            }
            finally
            {
                // Release sending lock.
                _wsSendSemaphore.Release();
            }
        }

        private void _startReceiveTask()
        {
            Task.Run(async () =>
            {
                var token = _wsTokenSource.Token;

                // If cancelled, disconnect and get out
                if (token.IsCancellationRequested)
                {
                    await Disconnect();
                }
                else
                {
                    while (_ws.State == WebSocketState.Open)
                    {
                        var buffer = new byte[1024];
                        var bytes = new List<byte>();
                        var str = "";

                        // First read all the message bytes
                        WebSocketMessageType messageType = WebSocketMessageType.Close;
                        bool endOfMessage = false;
                        do
                        {
                            try
                            {
                                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                                if (result != null)
                                {
                                    endOfMessage = result.EndOfMessage;
                                    var newBytes = new byte[result.Count];
                                    Array.Copy(buffer, newBytes, result.Count);
                                    bytes.AddRange(newBytes);
                                    messageType = result.MessageType;
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e, "Exception processing message from web socket.");
                                endOfMessage = true;
                            }

                        } while (!endOfMessage && _ws.State == WebSocketState.Open);

                        // Now process the message
                        switch (messageType)
                        {
                            case WebSocketMessageType.Close:
                                await Disconnect();
                                break;
                            case WebSocketMessageType.Text:
                                str += Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                                EmitMessage(new MessageEvent(str));
                                break;
                            case WebSocketMessageType.Binary:
                                EmitBinaryMessage(new BinaryMessageEvent(bytes.ToArray()));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                return Task.CompletedTask;
            }, _wsTokenSource.Token);
        }
        
        private void _stopWebSocketTasks()
        {
            _wsTokenSource.Cancel();
        }
    }
}
