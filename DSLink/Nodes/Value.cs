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
        private DateTime? _lastUpdated;

        /// <summary>
        /// Event occurs when value is set.
        /// </summary>
        public event Action<Value> OnSet;

        /// <summary>
        /// Event occurs when value is set only from a requester.
        /// </summary>
        public event Action<Value> OnRemoteSet;

        /// <summary>
        /// Gets last updated time in the ISO 8601 format that DSA uses.
        /// </summary>
        public string LastUpdated
        {
            get
            {
                if (!_lastUpdated.HasValue) return null;
                return _lastUpdated.Value.ToIso8601();
            }
        }

        public Value()
        {
            _val = null;
            _lastUpdated = null;
        }

        public Value(string val, DateTime? ts = null)
        {
            Set(val, false, ts);
        }

        public Value(bool val, DateTime? ts = null)
        {
            Set(val, false, ts);
        }

        public Value(int val, DateTime? ts = null)
        {
            Set(val, false, ts);
        }

        public Value(double val, DateTime? ts = null)
        {
            Set(val, false, ts);
        }

        public Value(float val, DateTime? ts = null)
        {
            Set(val, false, ts);
        }

        public Value(byte[] val, DateTime? ts = null)
        {
            Set(val, false, ts);
        }

        public Value(JToken val, DateTime? ts = null)
        {
            Set(val, false, ts);
        }

        public void Set(string val, bool force = false, DateTime? ts = null)
        {
            if (val.StartsWith("\x1B" + "bytes:") || val.StartsWith("\\u001bbytes:"))
            {
                var bytes = UrlBase64.Decode(val.Substring(val.IndexOf(":", StringComparison.Ordinal) + 1));
                Set(bytes, force);
            }
            else
            {
                if (!force && _val != null && _val.Value<string>() == val)
                {
                    return;
                }
                _val = val;
                _lastUpdated = ts;
            }
            SetValue();
        }

        public void Set(bool val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<bool>() == val)
            {
                return;
            }

            _val = val;
            _lastUpdated = ts;
            SetValue();
        }

        public void Set(int val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<int>() == val)
            {
                return;
            }

            _val = val;
            _lastUpdated = ts;
            SetValue();
        }

        public void Set(double val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<double>().Equals(val))
            {
                return;
            }

            _val = val;
            _lastUpdated = ts;
            SetValue();
        }

        public void Set(float val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<float>().Equals(val))
            {
                return;
            }

            _val = val;
            _lastUpdated = ts;
            SetValue();
        }

        public void Set(byte[] val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<byte[]>() == val)
            {
                return;
            }

            _val = val;
            _lastUpdated = ts;
            SetValue();
        }

        public void Set(DateTime val, bool force = false, DateTime? ts = null)
        {
            _val = val.ToIso8601();
            _lastUpdated = ts;
            SetValue();
        }

        public void Set(JToken jtoken, bool force = false, DateTime? ts = null)
        {
            _val = jtoken;
            _lastUpdated = ts;
            SetValue();
        }

        public void Clear()
        {
            _val = null;
            _lastUpdated = null;
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
            if (!_lastUpdated.HasValue)
                _lastUpdated = DateTime.Now;
            OnSet?.Invoke(this);
        }

        /// <summary>
        /// Used to invoke the remote set action from the
        /// Responder class.
        /// </summary>
        internal void InvokeRemoteSet()
        {
            OnRemoteSet?.Invoke(this);
        }
    }
}
