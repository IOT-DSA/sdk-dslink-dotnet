using System;

namespace DSLink.Util
{
    public static class TimeUtil
    {
        public static string ToIso8601(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
        }
    }
}
