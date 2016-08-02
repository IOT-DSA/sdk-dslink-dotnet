using System;
using DSLink.Util;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes
{
    /// <summary>
    /// Represents a Value.
    /// </summary>
    public class Value
    {
        /// <summary>
        /// Represents the value that this class is containing.
        /// </summary>
        private JToken _val;

        /// <summary>
        /// When the value was last updated.
        /// </summary>
        private DateTime _lastUpdated;

        /// <summary>
        /// Event occurs when value is set.
        /// </summary>
        public event Action<Value> OnSet;

        /// <summary>
        /// Gets last updated time in the ISO 8601 format that DSA uses.
        /// </summary>
        public string LastUpdated => TimeUtil.ToIso8601(_lastUpdated);

        public Value()
        {
            _val = null;
        }

        public Value(string val)
        {
            Set(val);
        }

        public Value(bool val)
        {
            Set(val);
        }

        public Value(int val)
        {
            Set(val);
        }

        public Value(double val)
        {
            Set(val);
        }

        public Value(float val)
        {
            Set(val);
        }

		public Value(byte[] val)
		{
			Set(val);
		}

        public Value(JToken val)
        {
            Set(val);
        }

        public void Set(string val)
        {
            if (val.StartsWith("\x1B" + "bytes:") || val.StartsWith("\\u001bbytes:"))
            {
                byte[] bytes = UrlBase64.Decode(val.Substring(val.IndexOf(":") + 1));
                _val = bytes;
                SetValue();
            }
            else
            {
                _val = val;
            }
            SetValue();
        }

        public void Set(bool val)
        {
            _val = val;
            SetValue();
        }

        public void Set(int val)
        {
            _val = val;
            SetValue();
        }

        public void Set(double val)
        {
            _val = val;
            SetValue();
        }

        public void Set(float val)
        {
            _val = val;
            SetValue();
        }

		public void Set(byte[] val)
		{
            _val = val;
			SetValue();
		}

        public void Set(JToken jtoken)
        {
            _val = jtoken;
            SetValue();
        }

        public JToken JToken => _val;
        public dynamic Dynamic => _val.Value<dynamic>();
        public bool Boolean => _val.Value<bool>();
        public string String => _val.Value<string>();
        public int Int => _val.Value<int>();
        public float Float => _val.Value<float>();
        public double Double => _val.Value<double>();
        public byte[] ByteArray => _val.Value<byte[]>();
        public JArray JArray => _val.Value<JArray>();

        /// <summary>
        /// Determines whether the value is null
        /// </summary>
        /// <value>True if the value is null</value>
        public bool IsNull => _val == null;

        /// <summary>
        /// Function to set updated time to now, and fire OnSet event.
        /// </summary>
        private void SetValue()
        {
            _lastUpdated = DateTime.Now;
            OnSet?.Invoke(this);
        }
    }
}
