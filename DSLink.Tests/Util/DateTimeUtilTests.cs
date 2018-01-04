using NUnit.Framework;
using DSLink.Util;
using System;
using FluentAssertions;

namespace DSLink.Tests.Util
{
    [TestFixture]
    public class DateTimeUtilTests
    {
        [Test]
        public void DateTimeToISO8601Format()
        {
            DateTime dateTime = new DateTime(2000, 1, 1);
            dateTime.ToIso8601().Should().StartWith("2000-01-01T00:00:00.000-");
        }
    }
}
