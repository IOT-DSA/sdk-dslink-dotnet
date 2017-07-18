using DSLink.Connection;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Respond;
using Moq;
using NUnit.Framework;

namespace DSLink.Tests
{
    [TestFixture]
    public class NodeChildrenTests
    {
        private Mock<AbstractContainer> _mockContainer;
        private Mock<Connector> _mockConnector;
        private Mock<Responder> _mockResponder;
        private Mock<SubscriptionManager> _mockSubManager;
        private Node _superRootNode;

        [SetUp]
        public void SetUp()
        {
            _mockContainer = new Mock<AbstractContainer>();
            _mockConnector = new Mock<Connector>(_mockContainer.Object);
            _mockResponder = new Mock<Responder>();
            _superRootNode = new Node("", null, _mockContainer.Object);

            _mockContainer.SetupGet(c => c.Connector).Returns(_mockConnector.Object);
            _mockContainer.SetupGet(c => c.Responder).Returns(_mockResponder.Object);
            _mockResponder.SetupGet(r => r.SuperRoot).Returns(_superRootNode);
        }

        [Test]
        public void TestCreateChildren()
        {
            int numberOfFirstLevelNodes = 5;
            int numberOfSecondLevelNodes = 100;
            for (int i = 0; i < numberOfFirstLevelNodes; i++)
            {
                var firstNode = _superRootNode.CreateChild($"Node{i}").BuildNode();
                for (int j = 0; j < numberOfSecondLevelNodes; j++)
                {
                    firstNode.CreateChild($"Node{j}").BuildNode();
                }
            }

            Assert.AreEqual(numberOfFirstLevelNodes, _superRootNode.Children.Count);
            foreach (var kv in _superRootNode.Children)
            {
                Assert.AreEqual(numberOfSecondLevelNodes, kv.Value.Children.Count);
            }
        }

        [Test]
        public void TestRemoveChildren()
        {
            const int childrenCount = 100;

            Assert.AreEqual(0, _superRootNode.Children.Count);

            for (int i = 0; i < childrenCount; i++)
            {
                _superRootNode.CreateChild($"Node{i}").BuildNode();
            }

            Assert.AreEqual(childrenCount, _superRootNode.Children.Count);

            for (int i = 0; i < childrenCount; i++)
            {
                _superRootNode[$"Node{i}"].RemoveFromParent();
            }

            Assert.AreEqual(0, _superRootNode.Children.Count);
        }

    }
}
