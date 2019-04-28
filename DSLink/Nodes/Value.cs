using System;
using System.Runtime.Serialization;
using DSLink.Util;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes
{
    /// <summary>
    /// Represents a Value.
    /// </summary>
    public class Value : ISerializable
    {
        /// <summary>
        /// Represents the value that this class is containing.
        /// </summary>
        private JToken _val;

        /// <summary>
        /// When the value was last updated.
        /// </summary>
        public DateTime? LastUpdated
        {
            get;
            private set;
        }

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
        public string LastUpdatedIso => LastUpdated?.ToIso8601();

        public Value()
        {
            _val = null;
            LastUpdated = null;
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

        public Value(DateTime val, DateTime? ts = null)
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
                LastUpdated = ts;
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
            LastUpdated = ts;
            SetValue();
        }

        public void Set(int val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<int>() == val)
            {
                return;
            }

            _val = val;
            LastUpdated = ts;
            SetValue();
        }

        public void Set(double val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<double>().Equals(val))
            {
                return;
            }

            _val = val;
            LastUpdated = ts;
            SetValue();
        }

        public void Set(float val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<float>().Equals(val))
            {
                return;
            }

            _val = val;
            LastUpdated = ts;
            SetValue();
        }

        public void Set(byte[] val, bool force = false, DateTime? ts = null)
        {
            if (!force && _val != null && _val.Value<byte[]>() == val)
            {
                return;
            }

            _val = val;
            LastUpdated = ts;
            SetValue();
        }

        public void Set(DateTime val, bool force = false, DateTime? ts = null)
        {
            var newValue = val.ToIso8601();
            if (!force && _val != null && _val.Value<string>() == newValue)
            {
                return;
            }

            _val = newValue;
            LastUpdated = ts;
            SetValue();
        }

        public void Set(JToken jtoken, bool force = false, DateTime? ts = null)
        {
            _val = jtoken;
            LastUpdated = ts;
            SetValue();
        }

        public void Clear()
        {
            _val = null;
            LastUpdated = null;
        }

        public T As<T>()
        {
            return _val.Value<T>();
        }

        public JToken AsJToken()
        {
            return _val;
        }

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
            if (!LastUpdated.HasValue)
            {
                LastUpdated = DateTime.Now;
            }

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

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Value", _val);
            info.AddValue("LastUpdated", LastUpdated);
        }

        public bool Equals(Value other)
        {
            return Equals(_val, other._val) && LastUpdated.Equals(other.LastUpdated);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (397 * (_val != null ? _val.GetHashCode() : 0)) ^ LastUpdated.GetHashCode();
            }
        }
    }
}