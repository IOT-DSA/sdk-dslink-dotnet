using DSLink.Connection;

namespace DSLink.UWP
{
    public class UWPPlatform
    {
        public static void Initialize()
        {
            Websockets.Universal.WebsocketConnection.Link();
            ConnectorManager.SetConnector(typeof(WebSocketBaseConnector));
        }
    }
}
