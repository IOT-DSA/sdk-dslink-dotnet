using DSLink.Connection;

namespace DSLink.Android
{
    public static class AndroidPlatform
    {
        public static void Initialize()
        {
            Websockets.Droid.WebsocketConnection.Link();
            ConnectorManager.SetConnector(typeof(WebSocketBaseConnector));
        }
    }
}
