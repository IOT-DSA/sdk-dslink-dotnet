using DSLink.Connection;
using DSLink.Util.Logger;

namespace DSLink.NET
{
    public static class NETPlatform
    {
        public static void Initialize()
        {
            Websockets.Net.WebsocketConnection.Link();
            ConnectorManager.SetConnector(typeof(WebSocketBaseConnector));
            BaseLogger.Logger = typeof(NETLogger);
        }
    }
}
