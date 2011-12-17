using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SharpOSC.Tests
{
	[TestFixture]
	public class TimetagTest
	{
		[TestCase]
		public void TestTimetag()
		{
			UInt64 time = (UInt64)60 * (UInt64)60 * (UInt64)24 * (UInt64)365 * (UInt64)108;
			time = time << 32;
			time = time + (UInt64)(Math.Pow(2, 32) / 2);
			var date = Utils.TimetagToDateTime(time);

			Assert.AreEqual(DateTime.Parse("2007-12-06 00:00:00.500"), date);
		}

		[TestCase]
		public void TestDateTimeToTimetag()
		{
			var dt = DateTime.Now;

			var l = Utils.DateTimeToTimetag(dt);
			var dtBack = Utils.TimetagToDateTime(l);

			Assert.AreEqual(dt.Date, dtBack.Date);
			Assert.AreEqual(dt.Hour, dtBack.Hour);
			Assert.AreEqual(dt.Minute, dtBack.Minute);
			Assert.AreEqual(dt.Second, dtBack.Second);
			Assert.AreEqual(dt.Millisecond, dtBack.Millisecond);
		}
	}
}
