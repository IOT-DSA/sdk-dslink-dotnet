using NUnit.Framework;
using DSLink.Nodes;
using UnitTests.Framework;

namespace UnitTests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void PathTest1()
        {
            TestingContainer tc = new TestingContainer("PathTest1", true);
            Node node = new Node("", null, tc);
            Assert.AreEqual(node.Path, "/");
        }

        [Test]
        public void PathTest2()
        {
            TestingContainer tc = new TestingContainer("PathTest2", true);
            Node node = new Node("", null, tc);
            Node child1 = node.CreateChild("Child1").BuildNode();
            Node child2 = node.CreateChild("Child2").BuildNode();
            Assert.AreEqual(child1.Path, "/Child1");
            Assert.AreEqual(child2.Path, "/Child2");
        }
    }
}
