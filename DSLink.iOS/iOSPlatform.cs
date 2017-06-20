using DSLink.Connection;
using DSLink.Platform;
using PCLStorage;
using System;

namespace DSLink.iOS
{
    public class iOSPlatform : BasePlatform
    {
        public static void Initialize()
        {
            SetPlatform(new iOSPlatform());
        }

        public override Connector CreateConnector(DSLinkContainer container)
        {
            return new iOSWebSocketConnector(container);
        }

        public override IFolder GetPlatformStorageFolder()
        {
            return FileSystem.Current.LocalStorage;
        }

        protected override Type GetLoggerType()
        {
            return typeof(iOSLogger);
        }
    }
}
