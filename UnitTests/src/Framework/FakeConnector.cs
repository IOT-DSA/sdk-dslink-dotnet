using DSLink;
using DSLink.Connection;
using DSLink.Connection.Serializer;

namespace UnitTests.Framework
{
    internal class FakeConnector : Connector
    {
        private bool _connected;

        public FakeConnector(Configuration config, ISerializer serializer) : base(config, serializer)
        {
        }

        public override void Connect()
        {
            _connected = true;
        }

        public override void Disconnect()
        {
            _connected = false;
        }

        public override bool Connected()
        {
            return _connected;
        }
    }
}
