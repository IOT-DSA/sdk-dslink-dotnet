using DSLink.Nodes;
using DSLink.Respond;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSLink.Protocol;

namespace DSLink.Test
{
    [TestFixture]
    public class NodeTests
    {
        private Mock<BaseLinkHandler> _mockContainer;
        private Mock<Connection> _mockConnector;
        private Mock<Responder> _mockResponder;
        private Mock<SubscriptionManager> _mockSubManager;
        private Node _superRootNode;

        [SetUp]
        public void SetUp()
        {
            _mockContainer = new Mock<BaseLinkHandler>(new Configuration("Test"));
            _mockConnector = new Mock<Connection>(
                _mockContainer.Object.Config
                //,
                //_mockContainer.Object.Logger
            );
            _mockResponder = new Mock<Responder>();
            _mockSubManager = new Mock<SubscriptionManager>(_mockContainer.Object);

            _mockContainer.SetupGet(c => c.Connection).Returns(_mockConnector.Object);
            _mockContainer.SetupGet(c => c.Responder).Returns(_mockResponder.Object);
            _mockResponder.SetupGet(r => r.SuperRoot).Returns(_superRootNode);
            _mockResponder.SetupGet(r => r.SubscriptionManager).Returns(_mockSubManager.Object);

            _mockConnector.Setup(c => c.AddValueUpdateResponse(It.IsAny<JToken>()))
                .Returns(Task.FromResult(false));

            _superRootNode = new Node("", null, _mockContainer.Object);
        }

        [Test]
        public void RemoveAllChildren()
        {
            var nodeA = _superRootNode.CreateChild("A").Build();
            nodeA.CreateChild("1").Build();
            nodeA.CreateChild("2").Build();
            nodeA.CreateChild("3").Build();
            var node4 = nodeA.CreateChild("4").Build();
            node4.CreateChild("4a").Build();
            node4.CreateChild("4b").Build();
            node4.CreateChild("4c").Build();

            nodeA.RemoveAllChildren();

            Assert.IsEmpty(nodeA.Children);
        }

        [Test]
        public void EachBannedCharacterInName()
        {
            foreach (char c in Node.BannedChars)
            {
                Assert.Throws<ArgumentException>(() => { _superRootNode.CreateChild($"Test{c}").Build(); });
            }
        }

        [Test]
        public void MultipleBannedCharactersInName()
        {
            var multiCharTest = new string(Node.BannedChars);
            Assert.Throws<ArgumentException>(() => { _superRootNode.CreateChild(multiCharTest).Build(); });
        }

        [Test]
        public void NoBannedCharactersInName()
        {
            var noCharTest = "TestNoBannedChars";
            Assert.DoesNotThrow(() => { _superRootNode.CreateChild(noCharTest).Build(); });
        }

        [Test]
        public void SubscriberValueUpdate()
        {
            var testValue = _superRootNode.CreateChild("TestValue")
                .SetType(DSLink.Nodes.ValueType.Number)
                .SetValue(0)
                .Build();

            _mockResponder.Object.SubscriptionManager.Subscribe(1, testValue);
            testValue.Value.Set(123);

            _mockConnector.Verify(c => c.AddValueUpdateResponse(
                It.Is<JToken>(
                    token => JToken.DeepEquals(token, new JArray
                    {
                        1,
                        testValue.Value.AsJToken(),
                        testValue.Value.LastUpdatedIso
                    })
                )
            ));
        }

        [Test]
        public void NodeTraversal()
        {
            var testParent = _superRootNode.CreateChild("testParent").Build();
            var testChild = testParent.CreateChild("testChild").Build();

            Assert.AreEqual(testParent, _superRootNode.Get(testParent.Path));
            Assert.AreEqual(testChild, _superRootNode.Get(testChild.Path));

            Assert.AreEqual(testParent, _superRootNode[testParent.Path]);
            Assert.AreEqual(testChild, _superRootNode[testChild.Path]);

            Assert.AreEqual(_superRootNode, testParent.Parent);
            Assert.AreEqual(_superRootNode, testChild.Parent.Parent);
        }

        [Test]
        public void GetMethodWithVariousPaths()
        {
            var testParent = _superRootNode.CreateChild("testParent").Build();
            var testChild = testParent.CreateChild("testChild").Build();

            Assert.AreEqual(testParent, _superRootNode.Get("/testParent/"));
            Assert.AreEqual(testParent, _superRootNode.Get("/testParent"));
            Assert.AreEqual(testParent, _superRootNode.Get("testParent"));

            Assert.AreEqual(testChild, _superRootNode.Get("/testParent/testChild/"));
            Assert.AreEqual(testChild, _superRootNode.Get("/testParent/testChild"));
            Assert.AreEqual(testChild, _superRootNode.Get("testParent/testChild/"));
            Assert.AreEqual(testChild, _superRootNode.Get("testParent/testChild"));
        }

        [Test]
        public void ConfigAttributeSerialization()
        {
            var testNode = _superRootNode
                .CreateChild("testNode")
                .SetConfig("number", new Value(123))
                .SetConfig("string", new Value("123"))
                .SetAttribute("number", new Value(123))
                .SetAttribute("string", new Value("123"))
                .Build();

            Assert.IsTrue(
                JToken.DeepEquals(_mockSubManager.Object.SerializeUpdates(_superRootNode), new JArray
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
        public void LocalSerialization()
        {
            var testNode = _superRootNode
                .CreateChild("testNode")
                .SetConfig("number", new Value(123))
                .SetConfig("string", new Value("123"))
                .SetAttribute("number", new Value(123))
                .SetAttribute("string", new Value("123"))
                .Build();

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
        public void LocalDeserialization()
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
            Assert.AreEqual(123, testNode.Configs.Get("number").As<int>());
            Assert.AreEqual("123", testNode.Configs.Get("string").As<string>());
            Assert.AreEqual(123, testNode.Attributes.Get("number").As<int>());
            Assert.AreEqual("123", testNode.Attributes.Get("string").As<string>());
        }
    }
}