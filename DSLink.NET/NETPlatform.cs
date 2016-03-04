using DSLink.Connection;
using DSLink.NET.Connection;

namespace DSLink.NET
{
    public class NetPlatform : Platform
    {
        public new static void Initialize()
        {
            ConnectorManager.SetConnector(typeof(WebSocketConnector));
        }
    }
}
