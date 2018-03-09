using NUnit.Framework;
using DSLink.Util;
using System;
using FluentAssertions;
using System.Text;

namespace DSLink.Test.Util
{
    [TestFixture]
    public class DateTimeUtilTests
    {
        [Test]
        public void DateTimeToISO8601Format()
        {
            DateTime dateTime = new DateTime(2000, 1, 1);
            var tzi = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            var hours = tzi.Hours;
            var mins = tzi.Minutes;
            var expectedDate = new StringBuilder("2000-01-01T00:00:00.000");
            expectedDate.Append(hours >= 0 ? '+' : '-');
            if (hours < 0) hours *= -1;
            expectedDate.Append(hours.ToString("00"));
            expectedDate.Append(':');
            expectedDate.Append(mins.ToString("00"));

            dateTime.ToIso8601().Should().Equals(expectedDate.ToString());
        }
    }
}
