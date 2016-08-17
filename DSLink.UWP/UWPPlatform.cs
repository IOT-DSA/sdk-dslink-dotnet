using DSLink.Connection;
using PCLStorage;

namespace DSLink.UWP
{
    public class UWPPlatform
    {
        public static void Initialize()
        {
            Configuration.StorageBaseFolder = FileSystem.Current.LocalStorage;
            Websockets.Universal.WebsocketConnection.Link();
            ConnectorManager.SetConnector(typeof(WebSocketBaseConnector));
        }
    }
}
