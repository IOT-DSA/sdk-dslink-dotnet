using DSLink.Nodes;
using NUnit.Framework;
using UnitTests.Framework;

namespace UnitTests.Tests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void PathTest1()
        {
            var tc = new TestingContainer("PathTest1", true);
            var node = new Node("", null, tc);
            Assert.AreEqual(node.Path, "/");
        }

        [Test]
        public void PathTest2()
        {
            var tc = new TestingContainer("PathTest2", true);
            var child1 = tc.Responder.SuperRoot.CreateChild("Child1").BuildNode();
            var child2 = tc.Responder.SuperRoot.CreateChild("Child2").BuildNode();
            Assert.AreEqual(child1.Path, "/Child1");
            Assert.AreEqual(child2.Path, "/Child2");
        }

        [Test]
        public void SerializeTest1()
        {
            var tc = new TestingContainer("SerializeTest1", true);
            var serialized = tc.Responder.SuperRoot.Serialize();
            Assert.AreEqual(serialized[0][0], "$is");
            Assert.AreEqual(serialized[0][1], "node");
        }

        [Test]
        public void AbuseStructure1()
        {
            var tc = new TestingContainer("AbuseStructure1", true);
            for (var i = 1; i <= 999; i++)
            {
                var node = tc.Responder.SuperRoot.CreateChild($"Test{i}").BuildNode();
                for (var j = 1; j <= 999; j++)
                {
                    node.CreateChild($"Test{j}").BuildNode();
                }
                Assert.AreEqual(node.Children.Count, 999);
            }
            Assert.AreEqual(tc.Responder.SuperRoot.Children.Count, 999);
        }
    }
}
