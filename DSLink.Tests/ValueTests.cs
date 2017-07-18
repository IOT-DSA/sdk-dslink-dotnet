using DSLink.Nodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DSLink.Tests
{
    [TestFixture]
    public class ValueTests
    {
        private static readonly byte[] _testBytes = {
            0x01, 0x02, 0x03, 0x04, 0x05
        };

        private static readonly byte[] _testBytesReverse = {
            0x05, 0x04, 0x03, 0x02, 0x01
        };

        [Test]
        public void TestNull_Created_IsNullTrue()
        {
            var value = new Value();
            Assert.IsTrue(value.IsNull);
        }

        [Test]
        public void TestNull_Created_IsJTokenNull()
        {
            var value = new Value();
            Assert.IsNull(value.JToken);
        }

        [Test]
        public void TestNull_AfterSetNumber_IsNullFalse()
        {
            var value = new Value();
            value.Set(123);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestBoolean_Created_BooleanPropertyEqualsValue()
        {
            var value = new Value(false);
            Assert.AreEqual(false, value.Boolean);
        }

        [Test]
        public void TestBoolean_Created_EqualsProperType()
        {
            var value = new Value(false);
            Assert.AreEqual(JTokenType.Boolean, value.JToken.Type);
        }

        [Test]
        public void TestBoolean_Created_IsNullFalse()
        {
            var value = new Value(false);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestBoolean_AfterSet_BooleanPropertyEqualsValue()
        {
            var value = new Value(false);
            value.Set(true);
            Assert.AreEqual(true, value.Boolean);
        }

        [Test]
        public void TestBoolean_AfterSet_EqualsProperType()
        {
            var value = new Value(false);
            value.Set(true);
            Assert.AreEqual(JTokenType.Boolean, value.JToken.Type);
        }

        [Test]
        public void TestBoolean_AfterSet_IsNullFalse()
        {
            var value = new Value(false);
            value.Set(true);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestInt_Created_IsNullFalse()
        {
            var value = new Value(123);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestInt_Created_IntPropertyEqualsValue()
        {
            var value = new Value(123);
            Assert.AreEqual(123, value.Int);
        }

        [Test]
        public void TestInt_Created_EqualsProperType()
        {
            var value = new Value(123);
            Assert.AreEqual(JTokenType.Integer, value.JToken.Type);
        }

        [Test]
        public void TestInt_AfterSet_IsNullFalse()
        {
            var value = new Value(123);
            value.Set(321);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestInt_AfterSet_IntPropertyEqualsValue()
        {
            var value = new Value(123);
            value.Set(321);
            Assert.AreEqual(321, value.Int);
        }

        [Test]
        public void TestInt_AfterSet_EqualsProperType()
        {
            var value = new Value(123);
            value.Set(321);
            Assert.AreEqual(JTokenType.Integer, value.JToken.Type);
        }

        [Test]
        public void TestFloat_Created_IsNullFalse()
        {
            var value = new Value(123.456f);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestFloat_Created_FloatPropertyEqualsValue()
        {
            var value = new Value(123.456f);
            Assert.AreEqual(123.456f, value.Float);
        }

        [Test]
        public void TestFloat_Created_EqualsProperType()
        {
            var value = new Value(123.456f);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
        }

        [Test]
        public void TestFloat_AfterSet_IsNullFalse()
        {
            var value = new Value(123.456f);
            value.Set(654.321f);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestFloat_AfterSet_FloatPropertyEqualsValue()
        {
            var value = new Value(123.456f);
            value.Set(654.321f);
            Assert.AreEqual(654.321f, value.Float);
        }

        [Test]
        public void TestFloat_AfterSet_EqualsProperType()
        {
            var value = new Value(123.456f);
            value.Set(654.321f);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
        }

        [Test]
        public void TestDouble_Created_IsNullFalse()
        {
            var value = new Value(123.456);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestDouble_Created_DoublePropertyEqualsValue()
        {
            var value = new Value(123.456);
            Assert.AreEqual(123.456, value.Double);
        }

        [Test]
        public void TestDouble_Created_EqualsProperType()
        {
            var value = new Value(123.456);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
        }

        [Test]
        public void TestDouble_AfterSet_IsNullFalse()
        {
            var value = new Value(123.456);
            value.Set(654.321);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestDouble_AfterSet_DoublePropertyEqualsValue()
        {
            var value = new Value(123.456);
            value.Set(654.321);
            Assert.AreEqual(654.321, value.Double);
        }

        [Test]
        public void TestDouble_AfterSet_EqualsProperType()
        {
            var value = new Value(123.456);
            value.Set(654.321);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
        }

        [Test]
        public void TestString_Created_IsNullFalse()
        {
            var value = new Value("123");
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestString_Created_StringPropertyEqualsValue()
        {
            var value = new Value("123");
            Assert.AreEqual("123", value.String);
        }

        public void TestString_Created_EqualsProperType()
        {
            var value = new Value("123");
            Assert.AreEqual(JTokenType.String, value.JToken.Type);
        }

        [Test]
        public void TestString_AfterSet_IsNullFalse()
        {
            var value = new Value("123");
            value.Set("321");
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestString_AfterSet_StringPropertyEqualsValue()
        {
            var value = new Value("123");
            value.Set("321");
            Assert.AreEqual("321", value.String);
        }

        public void TestString_AfterSet_EqualsProperType()
        {
            var value = new Value("123");
            value.Set("321");
            Assert.AreEqual(JTokenType.String, value.JToken.Type);
        }

        [Test]
        public void TestByteArray()
        {
            var value = new Value(_testBytes);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Bytes, value.JToken.Type);
            Assert.AreEqual(_testBytes, value.ByteArray);

            value.Set(_testBytesReverse);

            Assert.IsFalse(value.IsNull);
            Assert.AreEqual(JTokenType.Bytes, value.JToken.Type);
            Assert.AreEqual(_testBytesReverse, value.ByteArray);
        }

        [Test]
        public void TestByteArray_Created_IsNullFalse()
        {
            var value = new Value(_testBytes);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestByteArray_Created_ByteArrayPropertyEqualsValue()
        {
            var value = new Value(_testBytes);
            Assert.AreEqual(_testBytes, value.ByteArray);
        }

        [Test]
        public void TestByteArray_Created_EqualsProperType()
        {
            var value = new Value(_testBytes);
            Assert.AreEqual(JTokenType.Bytes, value.JToken.Type);
        }

        [Test]
        public void TestByteArray_AfterSet_IsNullFalse()
        {
            var value = new Value(_testBytes);
            value.Set(_testBytesReverse);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void TestByteArray_AfterSet_ByteArrayPropertyEqualsValue()
        {
            var value = new Value(_testBytes);
            value.Set(_testBytesReverse);
            Assert.AreEqual(_testBytesReverse, value.ByteArray);
        }

        [Test]
        public void TestByteArray_AfterSet_EqualsProperType()
        {
            var value = new Value(_testBytes);
            value.Set(_testBytesReverse);
            Assert.AreEqual(JTokenType.Bytes, value.JToken.Type);
        }
    }
}
