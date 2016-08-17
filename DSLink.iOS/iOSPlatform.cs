using DSLink.Connection;
using DSLink.Util.Logger;
using PCLStorage;

namespace DSLink.iOS
{
    public static class iOSPlatform
    {
        public static void Initialize()
        {
            Configuration.StorageBaseFolder = FileSystem.Current.LocalStorage;
            ConnectorManager.SetConnector(typeof(iOSWebSocketConnector));
            BaseLogger.Logger = typeof(iOSLogger);
        }
    }
}
