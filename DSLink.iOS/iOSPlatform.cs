using DSLink.Connection;
using DSLink.Respond;
using DSLink.Util.Logger;
using PCLStorage;

namespace DSLink.iOS
{
    public static class iOSPlatform
    {
        public static void Initialize()
        {
            Responder.StorageFolder = FileSystem.Current.LocalStorage;
            ConnectorManager.SetConnector(typeof(iOSWebSocketConnector));
            BaseLogger.Logger = typeof(iOSLogger);
        }
    }
}
