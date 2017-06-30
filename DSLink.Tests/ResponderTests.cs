using DSLink.Connection;
using DSLink.Container;
using DSLink.NET;
using DSLink.Platform;
using DSLink.Respond;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using PCLStorage;

namespace DSLink.Tests
{
    [TestFixture]
    public class ResponderTests
    {
        private Configuration _config;
        private DSLinkResponder _responder;
        private Mock<IFolder> _mockFolder;
        private Mock<AbstractContainer> _mockContainer;
        private Mock<Connector> _mockConnector;

        [SetUp]
        public void SetUp()
        {
            _mockFolder = new Mock<IFolder>();

            BasePlatform.SetPlatform(new TestPlatform(_mockFolder));
            _config = new Configuration(new List<string>(), "Test", responder: true);

            _mockContainer = new Mock<AbstractContainer>();
            _mockConnector = new Mock<Connector>(_mockContainer.Object);

            _mockContainer.SetupGet(c => c.Config).Returns(_config);
            _mockContainer.SetupGet(c => c.Connector).Returns(_mockConnector.Object);

            _responder = new DSLinkResponder(_mockContainer.Object);
        }

        [Test]
        public void TestyTest()
        {
        }

        public class TestPlatform : NETPlatform
        {
            private readonly Mock<IFolder> _mockFolder;

            public TestPlatform(Mock<IFolder> mockFolder)
            {
                _mockFolder = mockFolder;
            }

            public override IFolder GetPlatformStorageFolder()
            {
                return _mockFolder.Object;
            }
        }
    }
}
