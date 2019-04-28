using DSLink.Nodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DSLink.Test
{
    [TestFixture]
    public class ValueTests
    {
        private static readonly byte[] TestBytes =
        {
            0x01, 0x02, 0x03, 0x04, 0x05
        };

        private static readonly byte[] TestBytesReverse =
        {
            0x05, 0x04, 0x03, 0x02, 0x01
        };

        [Test]
        public void Null_Created_IsNullTrue()
        {
            var value = new Value();
            Assert.IsTrue(value.IsNull);
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
            Assert.AreEqual(false, value.As<bool>());
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
            Assert.AreEqual(true, value.As<bool>());
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
            Assert.AreEqual(123, value.As<int>());
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
            Assert.AreEqual(321, value.As<int>());
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
            Assert.AreEqual(123.456f, value.As<float>());
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
            Assert.AreEqual(654.321f, value.As<float>());
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
            Assert.AreEqual(123.456, value.As<double>());
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
            Assert.AreEqual(654.321, value.As<double>());
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
            Assert.AreEqual("123", value.As<string>());
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
            Assert.AreEqual("321", value.As<string>());
        }

        [Test]
        public void ByteArray_Created_IsNullFalse()
        {
            var value = new Value(TestBytes);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void ByteArray_Created_ByteArrayPropertyEqualsValue()
        {
            var value = new Value(TestBytes);
            Assert.AreEqual(TestBytes, value.As<byte[]>());
        }

        [Test]
        public void ByteArray_AfterSet_IsNullFalse()
        {
            var value = new Value(TestBytes);
            value.Set(TestBytesReverse);
            Assert.IsFalse(value.IsNull);
        }

        [Test]
        public void ByteArray_AfterSet_ByteArrayPropertyEqualsValue()
        {
            var value = new Value(TestBytes);
            value.Set(TestBytesReverse);
            Assert.AreEqual(TestBytesReverse, value.As<byte[]>());
        }
    }
}