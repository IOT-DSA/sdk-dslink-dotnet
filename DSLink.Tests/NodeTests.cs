using DSLink.Nodes;
using NUnit.Framework;
using System;

namespace DSLink.Tests
{
    [TestFixture]
    public class NodeTests
    {
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
    }
}
