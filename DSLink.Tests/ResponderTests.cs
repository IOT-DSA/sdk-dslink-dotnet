using DSLink.Connection;
using DSLink.Container;
using DSLink.NET;
using DSLink.Platform;
using DSLink.Respond;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace DSLink.Tests
{
    [TestFixture]
    public class ResponderTests
    {
        private Configuration _config;
        private Responder _responder;
        private Mock<BasePlatform> _mockPlatform;
        private Mock<AbstractContainer> _mockContainer;
        private Mock<Connector> _mockConnector;

        [SetUp]
        public void SetUp()
        {
            NETPlatform.Initialize();

            _config = new Configuration(new List<string>(), "Test", responder: true);

            _mockPlatform = new Mock<BasePlatform>();
            _mockContainer = new Mock<AbstractContainer>();
            _mockConnector = new Mock<Connector>(_mockContainer.Object);

            _mockContainer.SetupGet(c => c.Config).Returns(_config);
            _mockContainer.SetupGet(c => c.Connector).Returns(_mockConnector.Object);

            _responder = new Responder(_mockContainer.Object);
        }

        [Test]
        public void TestyTest()
        {
        }
    }
}
