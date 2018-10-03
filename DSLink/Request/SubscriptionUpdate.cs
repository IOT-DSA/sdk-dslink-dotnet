using System;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace DSLink.Request
{
    public class SubscriptionUpdate
    {
        public readonly int SubscriptionID;
        public readonly JToken Value;
        public readonly DateTime Updated;
        public readonly string Status;
        public readonly int? Count;
        public readonly double? Sum;
        public readonly double? Min;
        public readonly double? Max;

        public SubscriptionUpdate(int subID, JToken value, string updated)
        {
            SubscriptionID = subID;
            Value = value;
            Updated = DateTime.Parse(updated, null, DateTimeStyles.RoundtripKind);
        }

        public SubscriptionUpdate(int subID, string status, string updated)
        {
            SubscriptionID = subID;
            Status = status;
            Updated = DateTime.Parse(updated, null, DateTimeStyles.RoundtripKind);
        }

        public SubscriptionUpdate(int subID, JToken value, string updated, int count, double sum, double min, double max)
        {
            SubscriptionID = subID;
            Value = value;
            Updated = DateTime.Parse(updated, null, DateTimeStyles.RoundtripKind);
            Count = count;
            Sum = sum;
            Min = min;
            Max = max;
        }
    }
}

