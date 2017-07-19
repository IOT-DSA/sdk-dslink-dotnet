using DSLink.Connection;
using DSLink.Platform;
using PCLStorage;

namespace DSLink.UWP
{
    public class UWPPlatform : BasePlatform
    {
        public static void Initialize()
        {
            SetPlatform(new UWPPlatform());
        }

        public override void Init()
        {
            Websockets.Universal.WebsocketConnection.Link();
        }

        public override Connector CreateConnector(DSLinkContainer container)
        {
            return new WebSocketBaseConnector(container.Config, container.Logger);
        }

        public override IFolder GetPlatformStorageFolder()
        {
            return FileSystem.Current.LocalStorage;
        }

        public override string GetCommunicationFormat()
        {
            return "json";
        }
    }
}
