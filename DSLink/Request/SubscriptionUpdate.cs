using System;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace DSLink.Request
{
    public class SubscriptionUpdate
    {
        public readonly int SubscriptionId;
        public readonly JToken Value;
        public readonly DateTime Updated;
        public readonly string Status;
        public readonly int? Count;
        public readonly double? Sum;
        public readonly double? Min;
        public readonly double? Max;

        public SubscriptionUpdate(int subId, JToken value, string updated)
        {
            SubscriptionId = subId;
            Value = value;
            Updated = DateTime.Parse(updated, null, DateTimeStyles.RoundtripKind);
        }

        public SubscriptionUpdate(int subId, string status, string updated)
        {
            SubscriptionId = subId;
            Status = status;
            Updated = DateTime.Parse(updated, null, DateTimeStyles.RoundtripKind);
        }

        public SubscriptionUpdate(int subId, JToken value, string updated, int count, double sum, double min,
            double max)
        {
            SubscriptionId = subId;
            Value = value;
            Updated = DateTime.Parse(updated, null, DateTimeStyles.RoundtripKind);
            Count = count;
            Sum = sum;
            Min = min;
            Max = max;
        }
    }
}