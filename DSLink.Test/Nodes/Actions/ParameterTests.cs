using DSLink.Nodes;
using DSLink.Nodes.Actions;
using NUnit.Framework;
using FluentAssertions;
using FluentAssertions.Common;

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

            parameter.Name.Should().Be("testString");
        }

        [Test]
        public void TypeSetsProperly()
        {
            var parameter = new Parameter
            {
                ValueType = ValueType.Binary
            };

            parameter.ValueType.Should().Be(ValueType.Binary);
        }

        [Test]
        public void DefaultValueSetsProperly()
        {
            var actualValue = new Value(123);
            var parameter = new Parameter
            {
                DefaultValue = actualValue
            };
            var expectedValue = new Value(actualValue.As<int>(), actualValue.LastUpdated);

            parameter.DefaultValue.Should().BeEquivalentTo(expectedValue);
        }

        [Test]
        public void EditorTypeSetsProperly()
        {
            var parameter = new Parameter
            {
                Editor = EditorType.Color
            };

            parameter.Editor.Should().Be(EditorType.Color);
        }
    }
}