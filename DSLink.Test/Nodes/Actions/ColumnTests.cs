using DSLink.Nodes;
using DSLink.Nodes.Actions;
using NUnit.Framework;

namespace DSLink.Test.Nodes.Actions
{
    public class ColumnTests
    {
        [Test]
        public void NameSetsProperly()
        {
            var column = new Column
            {
                Name = "test"
            };

            Assert.AreEqual("test", column.Name);
        }

        [Test]
        public void ValueTypeSetsProperly()
        {
            var column = new Column
            {
                ValueType = ValueType.Binary
            };

            Assert.AreEqual(ValueType.Binary, column.ValueType);
        }
    }
}
