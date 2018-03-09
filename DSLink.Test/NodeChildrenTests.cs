using System.Collections.Generic;
using DSLink.Connection;
using DSLink.Nodes;
using DSLink.Respond;
using Moq;
using NUnit.Framework;

namespace DSLink.Test
{
    [TestFixture]
    public class NodeChildrenTests
    {
        private Mock<DSLinkContainer> _mockContainer;
        private Mock<Connector> _mockConnector;
        private Mock<Responder> _mockResponder;
        private Mock<SubscriptionManager> _mockSubManager;
        private Node _superRootNode;

        [SetUp]
        public void SetUp()
        {
            var config = new Configuration(new List<string>(), "Test");

            _mockContainer = new Mock<DSLinkContainer>(config);
            _mockConnector = new Mock<Connector>(
                _mockContainer.Object.Config,
                _mockContainer.Object.Logger
            );
            _mockResponder = new Mock<Responder>();
            _mockSubManager = new Mock<SubscriptionManager>(_mockContainer.Object);

            _mockContainer.SetupGet(c => c.Connector).Returns(_mockConnector.Object);
            _mockContainer.SetupGet(c => c.Responder).Returns(_mockResponder.Object);
            _mockResponder.SetupGet(r => r.SuperRoot).Returns(_superRootNode);
            _mockResponder.SetupGet(r => r.SubscriptionManager).Returns(_mockSubManager.Object);

            _superRootNode = new Node("", null, _mockContainer.Object);
        }

        [Test]
        public void CreateChildren()
        {
            const int numberOfFirstLevelNodes = 5;
            const int numberOfSecondLevelNodes = 100;
            for (var i = 0; i < numberOfFirstLevelNodes; i++)
            {
                var firstNode = _superRootNode.CreateChild($"Node{i}").BuildNode();
                for (var j = 0; j < numberOfSecondLevelNodes; j++)
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
        public void RemoveChildren()
        {
            const int childrenCount = 100;

            Assert.AreEqual(0, _superRootNode.Children.Count);

            for (var i = 0; i < childrenCount; i++)
            {
                _superRootNode.CreateChild($"Node{i}").BuildNode();
            }

            Assert.AreEqual(childrenCount, _superRootNode.Children.Count);

            for (var i = 0; i < childrenCount; i++)
            {
                _superRootNode[$"Node{i}"].RemoveFromParent();
            }

            Assert.AreEqual(0, _superRootNode.Children.Count);
        }
    }
}
