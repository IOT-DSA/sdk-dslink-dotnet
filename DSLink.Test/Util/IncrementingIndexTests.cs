using DSLink.Util;
using NUnit.Framework;

namespace DSLink.Test.Util
{
    [TestFixture]
    public class IncrementingIndexTests
    {
        [Test]
        public void IncrementFromDefaultZeroThreeTimes()
        {
            var inc = new IncrementingIndex();

            Assert.AreEqual(0, inc.Current);
            var i = inc.Next;
            Assert.AreEqual(1, inc.Current);
            i = inc.Next;
            Assert.AreEqual(2, inc.Current);
            i = inc.Next;
            Assert.AreEqual(3, inc.Current);
        }

        [Test]
        public void IncrementFromOneThreeTimes()
        {
            var inc = new IncrementingIndex(1);

            Assert.AreEqual(1, inc.Current);
            var i = inc.Next;
            Assert.AreEqual(2, inc.Current);
            i = inc.Next;
            Assert.AreEqual(3, inc.Current);
            i = inc.Next;
            Assert.AreEqual(4, inc.Current);
        }
    }
}
