using DSLink.Nodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DSLink.Test
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
        public void Null_Created_IsNullTrue()
        {
            var value = new Value();
            Assert.IsTrue(value.IsNull);
        }

        [Test]
        public void Null_Created_IsJTokenNull()
        {
            var value = new Value();
            Assert.IsNull(value.JToken);
        }

        [Test]
        public void Null_AfterSetNumber_IsNullFalse()
        {
            var value = new Value();
            value.Set(123);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Boolean_Created_BooleanPropertyEqualsValue()
        {
            var value = new Value(false);
            Assert.AreEqual(false, value.Boolean);
        }

        [Test]
        public void Boolean_Created_EqualsProperType()
        {
            var value = new Value(false);
            Assert.AreEqual(JTokenType.Boolean, value.JToken.Type);
        }

        [Test]
        public void Boolean_Created_IsNullFalse()
        {
            var value = new Value(false);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Boolean_AfterSet_BooleanPropertyEqualsValue()
        {
            var value = new Value(false);
            value.Set(true);
            Assert.AreEqual(true, value.Boolean);
        }

        [Test]
        public void Boolean_AfterSet_EqualsProperType()
        {
            var value = new Value(false);
            value.Set(true);
            Assert.AreEqual(JTokenType.Boolean, value.JToken.Type);
        }

        [Test]
        public void Boolean_AfterSet_IsNullFalse()
        {
            var value = new Value(false);
            value.Set(true);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Int_Created_IsNullFalse()
        {
            var value = new Value(123);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Int_Created_IntPropertyEqualsValue()
        {
            var value = new Value(123);
            Assert.AreEqual(123, value.Int);
        }

        [Test]
        public void Int_Created_EqualsProperType()
        {
            var value = new Value(123);
            Assert.AreEqual(JTokenType.Integer, value.JToken.Type);
        }

        [Test]
        public void Int_AfterSet_IsNullFalse()
        {
            var value = new Value(123);
            value.Set(321);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Int_AfterSet_IntPropertyEqualsValue()
        {
            var value = new Value(123);
            value.Set(321);
            Assert.AreEqual(321, value.Int);
        }

        [Test]
        public void Int_AfterSet_EqualsProperType()
        {
            var value = new Value(123);
            value.Set(321);
            Assert.AreEqual(JTokenType.Integer, value.JToken.Type);
        }

        [Test]
        public void Float_Created_IsNullFalse()
        {
            var value = new Value(123.456f);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Float_Created_FloatPropertyEqualsValue()
        {
            var value = new Value(123.456f);
            Assert.AreEqual(123.456f, value.Float);
        }

        [Test]
        public void Float_Created_EqualsProperType()
        {
            var value = new Value(123.456f);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
        }

        [Test]
        public void Float_AfterSet_IsNullFalse()
        {
            var value = new Value(123.456f);
            value.Set(654.321f);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Float_AfterSet_FloatPropertyEqualsValue()
        {
            var value = new Value(123.456f);
            value.Set(654.321f);
            Assert.AreEqual(654.321f, value.Float);
        }

        [Test]
        public void Float_AfterSet_EqualsProperType()
        {
            var value = new Value(123.456f);
            value.Set(654.321f);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
        }

        [Test]
        public void Double_Created_IsNullFalse()
        {
            var value = new Value(123.456);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Double_Created_DoublePropertyEqualsValue()
        {
            var value = new Value(123.456);
            Assert.AreEqual(123.456, value.Double);
        }

        [Test]
        public void Double_Created_EqualsProperType()
        {
            var value = new Value(123.456);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
        }

        [Test]
        public void Double_AfterSet_IsNullFalse()
        {
            var value = new Value(123.456);
            value.Set(654.321);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void Double_AfterSet_DoublePropertyEqualsValue()
        {
            var value = new Value(123.456);
            value.Set(654.321);
            Assert.AreEqual(654.321, value.Double);
        }

        [Test]
        public void Double_AfterSet_EqualsProperType()
        {
            var value = new Value(123.456);
            value.Set(654.321);
            Assert.AreEqual(JTokenType.Float, value.JToken.Type);
        }

        [Test]
        public void String_Created_IsNullFalse()
        {
            var value = new Value("123");
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void String_Created_StringPropertyEqualsValue()
        {
            var value = new Value("123");
            Assert.AreEqual("123", value.String);
        }

        [Test]
        public void String_Created_EqualsProperType()
        {
            var value = new Value("123");
            Assert.AreEqual(JTokenType.String, value.JToken.Type);
        }

        [Test]
        public void String_AfterSet_IsNullFalse()
        {
            var value = new Value("123");
            value.Set("321");
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void String_AfterSet_StringPropertyEqualsValue()
        {
            var value = new Value("123");
            value.Set("321");
            Assert.AreEqual("321", value.String);
        }

        [Test]
        public void String_AfterSet_EqualsProperType()
        {
            var value = new Value("123");
            value.Set("321");
            Assert.AreEqual(JTokenType.String, value.JToken.Type);
        }

        [Test]
        public void ByteArray_Created_IsNullFalse()
        {
            var value = new Value(_testBytes);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void ByteArray_Created_ByteArrayPropertyEqualsValue()
        {
            var value = new Value(_testBytes);
            Assert.AreEqual(_testBytes, value.ByteArray);
        }

        [Test]
        public void ByteArray_Created_EqualsProperType()
        {
            var value = new Value(_testBytes);
            Assert.AreEqual(JTokenType.Bytes, value.JToken.Type);
        }

        [Test]
        public void ByteArray_AfterSet_IsNullFalse()
        {
            var value = new Value(_testBytes);
            value.Set(_testBytesReverse);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void ByteArray_AfterSet_ByteArrayPropertyEqualsValue()
        {
            var value = new Value(_testBytes);
            value.Set(_testBytesReverse);
            Assert.AreEqual(_testBytesReverse, value.ByteArray);
        }

        [Test]
        public void ByteArray_AfterSet_EqualsProperType()
        {
            var value = new Value(_testBytes);
            value.Set(_testBytesReverse);
            Assert.AreEqual(JTokenType.Bytes, value.JToken.Type);
        }
    }
}
