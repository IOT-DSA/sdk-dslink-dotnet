using DSLink.Connection;
using DSLink.Util.Logger;

namespace DSLink.iOS
{
    public static class iOSPlatform
    {
        public static void Initialize()
        {
            ConnectorManager.SetConnector(typeof(iOSWebSocketConnector));
            BaseLogger.Logger = typeof(iOSLogger);
        }
    }
}
