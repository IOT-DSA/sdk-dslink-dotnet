using DSLink.Nodes;
using DSLink.Nodes.Actions;
using NUnit.Framework;

namespace DSLink.Test.Nodes.Actions
{
    public class ParameterTests
    {
        [Test]
        public void NameSetsProperly()
        {
            var parameter = new Parameter
            {
                Name = "testString"
            };

            Assert.AreEqual("testString", parameter.Name);
        }

        [Test]
        public void TypeSetsProperly()
        {
            var parameter = new Parameter
            {
                ValueType = ValueType.Binary
            };

            Assert.AreEqual(ValueType.Binary, parameter.ValueType);
        }

        [Test]
        public void DefaultValueSetsProperly()
        {
            var parameter = new Parameter
            {
                DefaultValue = 123
            };

            Assert.AreEqual(123, parameter.DefaultValue);
        }

        [Test]
        public void EditorTypeSetsProperly()
        {
            var parameter = new Parameter
            {
                Editor = EditorType.Color
            };

            Assert.AreEqual(EditorType.Color, parameter.Editor);
        }
    }
}
