using DSLink.Nodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DSLink.Tests
{
    [TestFixture]
    public class ValueTests
    {
        [Test]
        public void TestNull()
        {
            var value = new Value();

            Assert.IsNull(value.JToken);
            Assert.IsTrue(value.IsNull);

            value.Set(123);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestBoolean()
        {
            var value = new Value(false);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Boolean, value.JToken.Type);
            Assert.AreEqual(false, value.Boolean);

            value.Set(true);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Boolean, value.JToken.Type);
            Assert.AreEqual(true, value.Boolean);
        }

        [Test]
        public void TestInt()
        {
            var value = new Value(123);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Integer, value.JToken.Type);
            Assert.AreEqual(123, value.Int);

            value.Set(321);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Integer, value.JToken.Type);
            Assert.AreEqual(321, value.Int);
        }

        [Test]
        public void TestFloat()
        {
            var value = new Value(123.456f);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
            Assert.AreEqual(123.456f, value.Float);

            value.Set(654.321f);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
            Assert.AreEqual(654.321f, value.Float);
        }

        [Test]
        public void TestDouble()
        {
            var value = new Value(123.456);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
            Assert.AreEqual(123.456, value.Double);

            value.Set(654.321);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
            Assert.AreEqual(654.321, value.Double);
        }

        [Test]
        public void TestString()
        {
            var value = new Value("123");

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.String, value.JToken.Type);
            Assert.AreEqual("123", value.String);

            value.Set("321");

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.String, value.JToken.Type);
            Assert.AreEqual("321", value.String);
        }

        [Test]
        public void TestByteArray()
        {
            var value = new Value(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05
            });

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Bytes, value.JToken.Type);
            Assert.AreEqual(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05
            }, value.ByteArray);

            value.Set(new byte[]
            {
                0x05, 0x04, 0x03, 0x02, 0x01
            });

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Bytes, value.JToken.Type);
            Assert.AreEqual(new byte[]
            {
                0x05, 0x04, 0x03, 0x02, 0x01
            }, value.ByteArray);
        }
    }
}
