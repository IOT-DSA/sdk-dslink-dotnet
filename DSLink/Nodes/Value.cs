using System;
using System.Collections;
using DSLink.Util;

namespace DSLink.Nodes
{
    /// <summary>
    /// Represents a Value.
    /// </summary>
    public class Value
    {
        /// <summary>
        /// Value.
        /// </summary>
        private dynamic _val;

        /// <summary>
        /// When the value was last updated.
        /// </summary>
        private DateTime _lastUpdated;

        /// <summary>
        /// Event occurs when value is set.
        /// </summary>
        public event Action<Value> OnSet;

        /// <summary>
        /// Gets last updated time in ISO 8601 format.
        /// </summary>
        /// <value>The last updated.</value>
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

        public Value(IList val)
        {
            Set(val);
        }

        public Value(IDictionary val)
        {
            Set(val);
        }

		public Value(byte[] val)
		{
			Set(val);
		}

        public dynamic Get()
        {
            return _val;
        }

        public void Set(string val)
        {
            if (val.StartsWith("\x1B" + "bytes:") || val.StartsWith("\\u001bbytes:"))
            {
                byte[] bytes = UrlBase64.Decode(val.Substring(val.IndexOf(":") + 1));
                SetValue(bytes);
            }
            else
            {
                SetValue(val);
            }
        }

        public void Set(bool val)
        {
            SetValue(val);
        }

        public void Set(int val)
        {
            SetValue(val);
        }

        public void Set(double val)
        {
            SetValue(val);
        }

        public void Set(float val)
        {
            SetValue(val);
        }

        public void Set(IList val)
        {
            SetValue(val);
        }

        public void Set(IDictionary val)
        {
            SetValue(val);
        }

		public void Set(byte[] val)
		{
			SetValue(val);
		}

        /// <summary>
        /// Low level set method.
        /// </summary>
        /// <param name="val">Value.</param>
        private void SetValue(dynamic val)
        {
            _lastUpdated = DateTime.Now;
            _val = val;
            OnSet?.Invoke(this);
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        public object Clone() => new Value(_val) {_lastUpdated = _lastUpdated};
    }
}
