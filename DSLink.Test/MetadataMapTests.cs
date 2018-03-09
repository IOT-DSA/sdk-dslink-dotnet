using DSLink.Nodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DSLink.Test
{
    public class MetadataMapTests
    {
        private MetadataMap _metadataMap;

        [Test]
        public void MetadataSet_WithStringKey()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set("test", new Value("val"));

            Assert.NotNull(_metadataMap.Get("test"));
        }

        [Test]
        public void MetadataSet_WithBaseTypeKey()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set(ConfigType.ClassName, new Value("val"));

            Assert.NotNull(_metadataMap.Get(ConfigType.ClassName));
        }

        [Test]
        public void MetadataGet_WithStringKey()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set("test", new Value("val"));

            Assert.AreEqual("val", _metadataMap.Get("test").String);
        }

        [Test]
        public void MetadataGet_WithBaseTypeKey()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set(ConfigType.ClassName, new Value("val"));

            Assert.AreEqual("val", _metadataMap.Get(ConfigType.ClassName).String);
        }

        [Test]
        public void MetadataHas_WithStringKey()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set("test", new Value("val"));

            Assert.IsTrue(_metadataMap.Has("test"));
        }

        [Test]
        public void MetadataHas_WithBaseTypeKey()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set(ConfigType.ClassName, new Value("val"));

            Assert.IsTrue(_metadataMap.Has(ConfigType.ClassName));
        }

        [Test]
        public void MetadataHas_WithStringKeyInvalid()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set("test", new Value("val"));

            Assert.IsFalse(_metadataMap.Has("not_test"));
        }

        [Test]
        public void MetadataHas_WithBaseTypeKeyInvalid()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set(ConfigType.ClassName, new Value("val"));

            Assert.IsFalse(_metadataMap.Has(ConfigType.DisplayName));
        }

        [Test]
        public void MetadataCount_With1Add()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set(ConfigType.ClassName, new Value("val"));

            Assert.AreEqual(1, _metadataMap.Count);
        }

        [Test]
        public void MetadataCount_With2Add()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set(ConfigType.ClassName, new Value("val"));
            _metadataMap.Set(ConfigType.DisplayName, new Value("name"));

            Assert.AreEqual(2, _metadataMap.Count);
        }

        [Test]
        public void MetadataCount_2Add2Remove()
        {
            _metadataMap = new MetadataMap("");
            _metadataMap.Set(ConfigType.ClassName, new Value("val"));
            _metadataMap.Set(ConfigType.DisplayName, new Value("name"));
            _metadataMap.Remove(ConfigType.ClassName);
            _metadataMap.Remove(ConfigType.DisplayName);

            Assert.AreEqual(0, _metadataMap.Count);
        }

        [Test]
        public void MetadataClear_2AddThenClear()
        {
            _metadataMap = new MetadataMap("");

            _metadataMap.Set(ConfigType.ClassName, new Value("val"));
            _metadataMap.Set(ConfigType.DisplayName, new Value("name"));
            _metadataMap.Clear();

            Assert.AreEqual(0, _metadataMap.Count);
        }

        [Test]
        public void MetadataCreateUpdateArray_2AddCheckEquals()
        {
            _metadataMap = new MetadataMap("$");

            _metadataMap.Set(ConfigType.ClassName, new Value("light"));
            _metadataMap.Set(ConfigType.DisplayName, new Value("bulb_abc"));

            Assert.IsTrue(JToken.DeepEquals(
                _metadataMap.CreateUpdateArray(),
                new JArray
                {
                    new JArray
                    {
                        "$is",
                        "light"
                    },
                    new JArray
                    {
                        "$name",
                        "bulb_abc"
                    }
                }
            ));
        }

        [Test]
        public void MetadataEnumerator_2Add()
        {
            _metadataMap = new MetadataMap("");

            _metadataMap.Set(ConfigType.ClassName, new Value("light"));
            _metadataMap.Set(ConfigType.DisplayName, new Value("bulb_abc"));

            var enumerator = _metadataMap.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual(ConfigType.ClassName.String, enumerator.Current.Key);
            enumerator.MoveNext();
            Assert.AreEqual(ConfigType.DisplayName.String, enumerator.Current.Key);
        }
    }
}
