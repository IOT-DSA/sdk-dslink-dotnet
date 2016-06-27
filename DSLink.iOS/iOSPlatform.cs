using DSLink.Connection;

namespace DSLink.iOS
{
    public static class iOSPlatform
    {
        public static void Initialize()
        {
            ConnectorManager.SetConnector(typeof(iOSWebSocketConnector));
        }
    }
}
