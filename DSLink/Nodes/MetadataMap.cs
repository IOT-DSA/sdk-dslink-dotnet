using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections;

namespace DSLink.Nodes
{
    public delegate void OnSetEventHandler();

    public class MetadataMap : IEnumerable<KeyValuePair<string, Value>>
    {
        private Dictionary<string, Value> _metadataDictionary = new Dictionary<string, Value>();
        private readonly string _prefix;

        public event OnSetEventHandler OnSet;
        public int Count => _metadataDictionary.Count;

        public MetadataMap(string prefix)
        {
            _prefix = prefix;
        }

        public Value this[string key]
        {
            get
            {
                return Get(key);
            }
        }

        public Value this[BaseType baseType]
        {
            get
            {
                return Get(baseType);
            }
        }

        public void Set(string key, Value value)
        {
            _metadataDictionary[_prefix + key] = value;
            OnSet?.Invoke();
        }

        public void Set(BaseType key, Value value)
        {
            Set(key.String, value);
        }

        public Value Get(string key)
        {
            if (!_metadataDictionary.ContainsKey(_prefix + key))
            {
                return null;
            }

            return _metadataDictionary[_prefix + key];
        }

        public Value Get(BaseType key)
        {
            return Get(key.String);
        }

        public bool Has(string key)
        {
            return _metadataDictionary.ContainsKey(_prefix + key);
        }

        public bool Has(BaseType key)
        {
            return Has(key.String);
        }

        public void Remove(string key)
        {
            _metadataDictionary.Remove(_prefix + key);
        }

        public void Remove(BaseType key)
        {
            Remove(key.String);
        }

        public void Clear()
        {
            _metadataDictionary.Clear();
        }

        public JArray CreateUpdateArray()
        {
            var array = new JArray();

            foreach (var kp in _metadataDictionary)
            {
                array.Add(new JArray
                {
                    kp.Key,
                    kp.Value.JToken
                });
            }

            return array;
        }

        public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, Value>>)_metadataDictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, Value>>)_metadataDictionary).GetEnumerator();
        }
    }
}
