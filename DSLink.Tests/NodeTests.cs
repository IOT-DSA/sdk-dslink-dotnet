using DSLink.Connection;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Respond;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;

namespace DSLink.Tests
{
    [TestFixture]
    public class NodeTests
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
            _mockSubManager = new Mock<SubscriptionManager>(_mockContainer.Object);
            _superRootNode = new Node("", null, _mockContainer.Object);

            _mockContainer.SetupGet(c => c.Connector).Returns(_mockConnector.Object);
            _mockContainer.SetupGet(c => c.Responder).Returns(_mockResponder.Object);
            _mockResponder.SetupGet(r => r.SuperRoot).Returns(_superRootNode);
            _mockResponder.SetupGet(r => r.SubscriptionManager).Returns(_mockSubManager.Object);
        }

        [Test]
        public void TestBannedCharacters()
        {
            var bannedChars = Node.BannedChars;
            foreach (char c in bannedChars)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    new Node($"Test{c}", null, null);
                });
            }

            var multiCharTest = new string(Node.BannedChars);
            Assert.Throws<ArgumentException>(() =>
            {
                new Node(multiCharTest, null, null);
            });

            var noCharTest = "TestNoBannedChars";
            Assert.DoesNotThrow(() =>
            {
                new Node(noCharTest, null, null);
            });
        }

        [Test]
        public void TestCreateChildren()
        {
            int first = 5; // Number of children of root to create
            int second = 100; // Number of children to make below each first node.
            var root = new Node("", null, null);
            for (int i = 0; i < first; i++)
            {
                var firstNode = root.CreateChild($"Node{i}").BuildNode();
                for (int j = 0; j < second; j++)
                {
                    firstNode.CreateChild($"Node{j}").BuildNode();
                }
            }

            Assert.AreEqual(first, root.Children.Count);
            foreach (var kv in root.Children)
            {
                Assert.AreEqual(second, kv.Value.Children.Count);
            }
        }

        [Test]
        public void TestSubscriberUpdate()
        {
            var testValue = _superRootNode.CreateChild("TestValue")
                .SetType(Nodes.ValueType.Number)
                .SetValue(0)
                .BuildNode();

            _mockResponder.Object.SubscriptionManager.Subscribe(1, testValue);

            //_mockConnector.Verify(c => c.Write(null), Times.Once());
            /*_mockConnector.Verify(
                c => c.Write(
                    It.Is<JObject>(
                        data =>
                        {
                        }
                    ),
                    false
                )
            );*/
        }
    }
}
