using System;
using System.Globalization;
using DSLink.Nodes;

namespace DSLink.Request
{
    public class SubscriptionUpdate
    {
        public readonly int SubscriptionID;
        public readonly dynamic Value;
        public readonly DateTime Updated;
        public readonly string Status;
        public readonly int? Count;
        public readonly int? Sum;
        public readonly int? Min;
        public readonly int? Max;

        public SubscriptionUpdate(int subID, dynamic value, string updated)
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

        public SubscriptionUpdate(int subID, dynamic value, string updated, int count, int sum, int min, int max)
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

