using DSLink.Connection;
using DSLink.Util.Logger;
using PCLStorage;

namespace DSLink.Android
{
    public static class AndroidPlatform
    {
        public static void Initialize()
        {
            Configuration.StorageBaseFolder = FileSystem.Current.LocalStorage;
            Websockets.Droid.WebsocketConnection.Link();
            ConnectorManager.SetConnector(typeof(WebSocketBaseConnector));
            BaseLogger.Logger = typeof(AndroidLogger);
        }
    }
}
