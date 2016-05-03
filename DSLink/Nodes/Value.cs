using System;
using System.Collections;
using DSLink.Util;

namespace DSLink.Nodes
{
    public class Value
    {
        private dynamic _val;
        private DateTime _lastUpdated;
        public event Action<Value> OnSet;

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

        public dynamic Get()
        {
            return _val;
        }

        public void Set(string val)
        {
            SetValue(val);
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

        private void SetValue(dynamic val)
        {
            _lastUpdated = DateTime.Now;
            _val = val;
            OnSet?.Invoke(this);
        }

        public object Clone() => new Value(_val) {_lastUpdated = _lastUpdated};
    }
}
