using DSLink.Connection;
using DSLink.Respond;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using StandardStorage;
using Newtonsoft.Json.Linq;
using DSLink.Nodes;
using System.Threading.Tasks;
using DSLink.Nodes.Actions;

namespace DSLink.Test
{
    [TestFixture]
    public class ResponderTests
    {
        private Configuration _config;
        private DSLinkResponder _responder;
        private Mock<IFolder> _mockFolder;
        private Mock<DSLinkContainer> _mockContainer;
        private Mock<SubscriptionManager> _mockSubManager;
        private Mock<Connector> _mockConnector;

        [SetUp]
        public void SetUp()
        {
            _mockFolder = new Mock<IFolder>();

            _config = new Configuration(new List<string>(), "Test", responder: true);

            _mockContainer = new Mock<DSLinkContainer>(new Configuration(new List<string>(), "Test"));
            _mockConnector = new Mock<Connector>(
                _mockContainer.Object.Config,
                _mockContainer.Object.Logger
            );

            _mockContainer.SetupGet(c => c.Connector).Returns(_mockConnector.Object);

            _responder = new DSLinkResponder(_mockContainer.Object);
            _mockContainer.SetupGet(c => c.Responder).Returns(_responder);
            _responder.Init();

            _responder.SuperRoot.CreateChild("testValue")
                .SetType(DSLink.Nodes.ValueType.Number)
                .SetValue(123)
                .BuildNode();

            _responder.SuperRoot.CreateChild("testNodeConfigs")
                .SetConfig("testString", new Value("string"))
                .SetConfig("testNumber", new Value(123))
                .BuildNode();
        }

        private Task<JArray> _listNode()
        {
            return _responder.ProcessRequests(new JArray
            {
                new JObject
                {
                    new JProperty("rid", 1),
                    new JProperty("method", "list"),
                    new JProperty("path", "/")
                }
            });
        }

        private Task<JArray> _invokeNode()
        {
            return _responder.ProcessRequests(new JArray
            {
                new JObject
                {
                    new JProperty("rid", 1),
                    new JProperty("method", "invoke"),
                    new JProperty("permit", "write"),
                    new JProperty("path", "/testAction"),
                    new JProperty("params", new JObject
                    {
                        new JProperty("testString", "string"),
                        new JProperty("testNumber", 123)
                    })
                }
            });
        }

        private Task<JArray> _subscribeToNode()
        {
            return _responder.ProcessRequests(new JArray
            {
                new JObject
                {
                    new JProperty("rid", 1),
                    new JProperty("method", "subscribe"),
                    new JProperty("paths", new JArray
                    {
                        new JObject
                        {
                            new JProperty("path", "/testValue"),
                            new JProperty("sid", 0)
                        }
                    })
                }
            });
        }

        private Task<JArray> _unsubscribeFromNode()
        {
            return _responder.ProcessRequests(new JArray
            {
                new JObject
                {
                    new JProperty("rid", 2),
                    new JProperty("method", "unsubscribe"),
                    new JProperty("sids", new JArray
                    {
                        0
                    })
                }
            });
        }

        private void _setUpNodeClass()
        {
            _responder.AddNodeClass("testClass", (Node node) =>
            {
                node.Configs.Set(ConfigType.DisplayName, new Value("test"));
                node.Attributes.Set("attr", new Value("test"));
            });
        }

        // TODO: Split this into multiple tests
        [Test]
        public async Task List()
        {
            var responses = await _listNode();
            var response = responses[0];
            var updates = response["updates"];

            Assert.AreEqual(1, response["rid"].Value<int>());
            Assert.AreEqual("open", response["stream"].Value<string>());

            Assert.IsTrue(JToken.DeepEquals(updates[0], new JArray
            {
                "$is",
                "node"
            }));

            var testValueUpdate = updates[1][1];
            Assert.AreEqual("testValue", updates[1][0].Value<string>());
            Assert.NotNull(testValueUpdate["$is"]);
            Assert.NotNull(testValueUpdate["$type"]);
            Assert.NotNull(testValueUpdate["value"]);
            Assert.NotNull(testValueUpdate["ts"]);
            Assert.AreEqual("node", testValueUpdate["$is"].Value<string>());
            Assert.AreEqual("number", testValueUpdate["$type"].Value<string>());
            Assert.AreEqual(123, testValueUpdate["value"].Value<int>());
            Assert.AreEqual(JTokenType.String, testValueUpdate["ts"].Type);

            var testNodeUpdate = updates[2][1];
            Assert.AreEqual("testNodeConfigs", updates[2][0].Value<string>());
            Assert.NotNull(testNodeUpdate["$is"]);
            Assert.NotNull(testNodeUpdate["$testString"]);
            Assert.NotNull(testNodeUpdate["$testNumber"]);
            Assert.AreEqual("node", testNodeUpdate["$is"].Value<string>());
            Assert.AreEqual("string", testNodeUpdate["$testString"].Value<string>());
            Assert.AreEqual(123, testNodeUpdate["$testNumber"].Value<int>());
        }

        // TODO: Split this into multiple tests
        [Test]
        public async Task Invoke()
        {
            bool actionInvoked = false;
            _responder.SuperRoot.CreateChild("testAction")
                .SetInvokable(Permission.Write)
                .SetAction(new ActionHandler(Permission.Write, async (request) =>
                {
                    actionInvoked = true;
                    await request.Close();
                }))
                .BuildNode();

            await _invokeNode();

            Assert.IsTrue(actionInvoked);
        }

        // TODO: Split this into multiple tests
        [Test]
        public async Task InvokeParameters()
        {
            _responder.SuperRoot.CreateChild("testAction")
                .SetInvokable(Permission.Write)
                .SetAction(new ActionHandler(Permission.Write, async (request) =>
                {
                    Assert.AreEqual("string", request.Parameters["testString"].Value<string>());
                    Assert.AreEqual(123, request.Parameters["testNumber"].Value<int>());
                    await request.Close();
                }))
                .BuildNode();

            await _invokeNode();
        }

        // TODO: Split this into multiple tests
        [Test]
        public async Task Subscribe()
        {
            var requestResponses = await _subscribeToNode();

            var requestUpdate = requestResponses[0];
            var update = requestUpdate["updates"][0];
            var requestClose = requestResponses[1];

            // Test for subscribe method value update.
            Assert.AreEqual(0, requestUpdate["rid"].Value<int>()); // Request ID
            Assert.AreEqual(0, update[0].Value<int>()); // Subscription ID
            Assert.AreEqual(123, update[1].Value<int>()); // Value
            Assert.IsInstanceOf(typeof(string), update[2].Value<string>()); // Timestamp TODO: Test if valid

            // Test for subscribe method stream close.
            Assert.AreEqual(1, requestClose["rid"].Value<int>());
            Assert.AreEqual("closed", requestClose["stream"].Value<string>());
        }

        // TODO: Split this into multiple tests
        [Test]
        public async Task Unsubscribe()
        {
            await _subscribeToNode();
            var requestResponses = await _unsubscribeFromNode();

            var requestClose = requestResponses[0];

            // Test for unsubscribe method stream close.
            Assert.AreEqual(2, requestClose["rid"].Value<int>());
            Assert.AreEqual("closed", requestClose["stream"].Value<string>());
        }

        [Test]
        public void NodeClassAdd()
        {
            _setUpNodeClass();

            var node = _responder.SuperRoot.CreateChild("testNodeClass", "testClass").BuildNode();

            Assert.AreEqual("test", node.Configs.Get(ConfigType.DisplayName).String);
            Assert.AreEqual("test", node.Attributes.Get("attr").String);
        }
    }
}
