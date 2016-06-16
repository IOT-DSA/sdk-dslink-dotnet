using DSLink.Connection;
using DSLink.Util.Logger;

namespace DSLink.Android
{
    public static class AndroidPlatform
    {
        public static void Initialize()
        {
            Websockets.Droid.WebsocketConnection.Link();
            ConnectorManager.SetConnector(typeof(WebSocketBaseConnector));
            BaseLogger.Logger = typeof(AndroidLogger);
        }
    }
}
