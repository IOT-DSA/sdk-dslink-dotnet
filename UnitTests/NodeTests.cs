using NUnit.Framework;
using DSLink.Nodes;

namespace UnitTests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void PathTest1()
        {
            // TODO: Fake out DSLinkContainer
            Node node = new Node("", null);
            Assert.AreEqual(node.Path, "/");
        }

        [Test]
        public void PathTest2()
        {
            // TODO: Fake out DSLinkContainer
            Node node = new Node("", null);
            Node child1 = node.CreateChild("Child1").BuildNode();
            Node child2 = node.CreateChild("Child2").BuildNode();
            Assert.AreEqual(child1.Path, "/Child1");
            Assert.AreEqual(child2.Path, "/Child2");
        }
    }
}
