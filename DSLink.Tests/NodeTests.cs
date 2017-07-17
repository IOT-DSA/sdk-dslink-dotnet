using DSLink.Connection;
using DSLink.Container;
using DSLink.Nodes;
using DSLink.Respond;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

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

            _mockConnector.Setup(c => c.AddValueUpdateResponse(It.IsAny<JToken>()))
                .Returns(Task.FromResult(false));
        }

        [Test]
        public void TestBannedCharacters()
        {
            var bannedChars = Node.BannedChars;
            foreach (char c in bannedChars)
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    _superRootNode.CreateChild($"Test{c}").BuildNode();
                });
            }

            var multiCharTest = new string(Node.BannedChars);
            Assert.Throws<ArgumentException>(() =>
            {
                _superRootNode.CreateChild(multiCharTest).BuildNode();
            });

            var noCharTest = "TestNoBannedChars";
            Assert.DoesNotThrow(() =>
            {
                _superRootNode.CreateChild(noCharTest).BuildNode();
            });
        }

        [Test]
        public void TestCreateChildren()
        {
            int first = 5; // Number of children of root to create
            int second = 100; // Number of children to make below each first node.
            for (int i = 0; i < first; i++)
            {
                var firstNode = _superRootNode.CreateChild($"Node{i}").BuildNode();
                for (int j = 0; j < second; j++)
                {
                    firstNode.CreateChild($"Node{j}").BuildNode();
                }
            }

            Assert.AreEqual(first, _superRootNode.Children.Count);
            foreach (var kv in _superRootNode.Children)
            {
                Assert.AreEqual(second, kv.Value.Children.Count);
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

        [Test]
        public void TestSubscriberUpdate()
        {
            var testValue = _superRootNode.CreateChild("TestValue")
                .SetType(Nodes.ValueType.Number)
                .SetValue(0)
                .BuildNode();

            _mockResponder.Object.SubscriptionManager.Subscribe(1, testValue);
            testValue.Value.Set(123);

            _mockConnector.Verify(c => c.AddValueUpdateResponse(
                It.Is<JToken>(
                    token => JToken.DeepEquals(token, new JArray
                    {
                        1,
                        testValue.Value.JToken,
                        testValue.Value.LastUpdated
                    })
                )
            ));
        }

        [Test]
        public void TestNodeTraversal()
        {
            var testParent = _superRootNode.CreateChild("testParent").BuildNode();
            var testChild = testParent.CreateChild("testChild").BuildNode();

            Assert.AreEqual(testParent, _superRootNode.Get(testParent.Path));
            Assert.AreEqual(testChild, _superRootNode.Get(testChild.Path));

            Assert.AreEqual(testParent, _superRootNode[testParent.Path]);
            Assert.AreEqual(testChild, _superRootNode[testChild.Path]);

            Assert.AreEqual(_superRootNode, testParent.Parent);
            Assert.AreEqual(_superRootNode, testChild.Parent.Parent);
        }

        [Test]
        public void TestConfigAttributeSerialization()
        {
            var testNode = _superRootNode
                .CreateChild("testNode")
                .SetConfig("number", new Value(123))
                .SetConfig("string", new Value("123"))
                .SetAttribute("number", new Value(123))
                .SetAttribute("string", new Value("123"))
                .BuildNode();

            Assert.IsTrue(
                JToken.DeepEquals(_superRootNode.SerializeUpdates(), new JArray
                {
                    new JArray
                    {
                        "$is",
                        "node"
                    },
                    new JArray
                    {
                        "testNode",
                        new JObject
                        {
                            new JProperty("$is", "node"),
                            new JProperty("$number", 123),
                            new JProperty("$string", "123"),
                            new JProperty("@number", 123),
                            new JProperty("@string", "123")
                        }
                    }
                })
            );
        }

        [Test]
        public void TestLocalSerialization()
        {
            var testNode = _superRootNode
                .CreateChild("testNode")
                .SetConfig("number", new Value(123))
                .SetConfig("string", new Value("123"))
                .SetAttribute("number", new Value(123))
                .SetAttribute("string", new Value("123"))
                .BuildNode();

            Assert.IsTrue(
                JToken.DeepEquals(_superRootNode.Serialize(), new JObject
                {
                    new JProperty("$is", "node"),
                    new JProperty("privateConfigs", new JObject()),
                    new JProperty("testNode", new JObject
                    {
                        new JProperty("$is", "node"),
                        new JProperty("$number", 123),
                        new JProperty("$string", "123"),
                        new JProperty("@number", 123),
                        new JProperty("@string", "123"),
                        new JProperty("privateConfigs", new JObject())
                    })
                })
            );
        }

        [Test]
        public void TestLocalDeserialization()
        {
            var testObject = new JObject
            {
                new JProperty("$is", "node"),
                new JProperty("privateConfigs", new JObject()),
                new JProperty("testNode", new JObject
                {
                    new JProperty("$is", "node"),
                    new JProperty("$number", 123),
                    new JProperty("$string", "123"),
                    new JProperty("@number", 123),
                    new JProperty("@string", "123"),
                    new JProperty("privateConfigs", new JObject())
                })
            };

            _superRootNode.Deserialize(testObject);

            Assert.IsNotNull(_superRootNode["testNode"]);
            var testNode = _superRootNode["testNode"];
            Assert.AreEqual(123, testNode.GetConfig("number").Int);
            Assert.AreEqual("123", testNode.GetConfig("string").String);
            Assert.AreEqual(123, testNode.GetAttribute("number").Int);
            Assert.AreEqual("123", testNode.GetAttribute("string").String);
        }
    }
}
