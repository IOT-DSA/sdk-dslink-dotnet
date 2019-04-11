using DSLink.Nodes;
using NUnit.Framework;

namespace DSLink.Test
{
    [TestFixture]
    public class ValueTypeTests
    {
        [Test]
        public void MakeEnum_Strings()
        {
            ValueType enm = ValueType.MakeEnum("123", "321", "456", "654");

            Assert.AreEqual("enum[123,321,456,654]", enm.Type);
        }
    }
}