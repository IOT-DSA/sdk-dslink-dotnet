using DSLink.Connection;
using DSLink.Respond;
using DSLink.Util.Logger;
using PCLStorage;

namespace DSLink.Android
{
    public static class AndroidPlatform
    {
        public static void Initialize()
        {
            Responder.StorageFolder = FileSystem.Current.LocalStorage;
            Websockets.Droid.WebsocketConnection.Link();
            ConnectorManager.SetConnector(typeof(WebSocketBaseConnector));
            BaseLogger.Logger = typeof(AndroidLogger);
        }
    }
}
