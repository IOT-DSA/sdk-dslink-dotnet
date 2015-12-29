using DSLink;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace UnitTests.Framework
{
    internal class TestingContainer : AbstractContainer
    {
        public TestingContainer(string testSuite, bool responder = false, bool requester = false)
            : base(new Configuration(testSuite, responder: responder, requester: requester))
        {
            CreateLogger($"DSLink-{testSuite}");
            
            Connector = new FakeConnector(Config, new JsonSerializer());
            Connector.Connect();
        }
    }
}
