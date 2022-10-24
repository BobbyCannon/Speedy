#region References

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscTimeTagTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void name()
		{
			var datetime = new DateTime(636835710121234567, DateTimeKind.Utc);
			datetime.ToString("O").Dump();
			var span = datetime.ToUniversalTime() - OscTimeTag.MinDateTime;
			var seconds = span.TotalSeconds;
			var secondsUInt = (uint) seconds;
			var remainingTicks = span.Ticks - ((decimal) secondsUInt * TimeSpan.TicksPerSecond);
			var fraction = ((remainingTicks / TimeSpan.TicksPerMillisecond) / 1000.0m) * uint.MaxValue;
			var test = ((ulong) (secondsUInt & 0xFFFFFFFF) << 32) | ((ulong) fraction & 0xFFFFFFFF);
			test.Dump();
			
			var timetag3 = new OscTimeTag(datetime);
			timetag3.Value.Dump();

			var timetag = new OscTimeTag(test);
			timetag.Seconds.Dump("seconds: ");
			timetag.SubSeconds.Dump("sub seconds: ");
			timetag.PreciseValue.Dump("precise1: ");

			var ticks1 = Math.Round((((decimal) timetag.SubSeconds) / uint.MaxValue) * 1000, 4, MidpointRounding.AwayFromZero);
			ticks1.Dump("Ticks1: ");
			var ticks2 = ticks1 * TimeSpan.TicksPerMillisecond;
			ticks2.Dump("Ticks2: ");

			var datetime2 = OscTimeTag.MinDateTime.AddSeconds(timetag.Seconds).AddTicks((long) ticks2);
			datetime2.ToString("O").Dump();

			var datetime3 = timetag.ToDateTime();
			datetime3.ToString("O").Dump();

			var timetag2 = datetime2.ToOscTimeTag();
			timetag2.PreciseValue.Dump("precise2: ");
			Assert.AreEqual(timetag.PreciseValue, timetag2.PreciseValue);
		}

		[TestMethod]
		public void AddTimeSpan()
		{
			var span = TimeSpan.FromMilliseconds(123);
			var datetime = new DateTime(2019, 1, 20, 08, 50, 12, DateTimeKind.Utc);
			var expected = new OscTimeTag(datetime.Add(span));
			var timetag = new OscTimeTag(datetime);

			Assert.AreEqual(expected, timetag.Add(span));
		}

		[TestMethod]
		public void Compare()
		{
			var expected = new DateTime(2019, 1, 20, 08, 50, 12, DateTimeKind.Utc);
			var time1 = OscTimeTag.FromDateTime(expected);
			var time2 = new OscTimeTag(16136033268821655552);
			Assert.IsTrue(time1 == time2);
			Assert.AreEqual(time1, time2);

			time1 = OscTimeTag.MinValue;
			time2 = OscTimeTag.MaxValue;
			Assert.IsTrue(time1 < time2);
			Assert.IsFalse(time1 > time2);

			time1 = OscTimeTag.MinValue;
			time2 = OscTimeTag.MinValue;
			Assert.IsTrue(time1 == time2);
			Assert.IsFalse(time1 != time2);

			time1 = OscTimeTag.MinValue;
			time2 = OscTimeTag.MinValue;
			Assert.IsTrue(time1 >= time2);
			Assert.IsTrue(time1 <= time2);
		}

		[TestMethod]
		public void FromDateTime()
		{
			var values = new List<(DateTime, ulong, string)>
			{
				//(new DateTime(2019, 1, 20, 08, 50, 12, DateTimeKind.Utc), 16136033268821655552, "2019-01-20T08:50:12.0000000Z"),
				(new DateTime(2019, 1, 20, 03, 50, 12, DateTimeKind.Local), 16136033268821655552, "2019-01-20T08:50:12.0000000Z"),
				//(new DateTime(636835710121234567, DateTimeKind.Utc), 16136033269351898040, "2019-01-20T08:50:12.1234567Z")
			};

			foreach (var e in values)
			{
				var d = e.Item3.ToUtcDateTime();
				d.Ticks.Dump();
				d.ToString("O").Dump();

				var actual = OscTimeTag.FromDateTime(e.Item1);
				actual.ToString("O").Dump();

				Assert.AreEqual(e.Item2, actual.Value);
				Assert.AreEqual(e.Item3, actual.ToString());
			}
		}

		[TestMethod]
		public void FromMillisecond()
		{
			var a = OscTimeTag.FromMilliseconds(1234);
			Assert.AreEqual(1234, a.ToMilliseconds());
			Assert.AreEqual("1900-01-01T00:00:01.2340000Z", a.ToString());
			Assert.AreEqual(5299989643u, a.Value);

			a = new OscTimeTag(5299989643);
			Assert.AreEqual(1234, a.ToMilliseconds());
			Assert.AreEqual("1900-01-01T00:00:01.2340000Z", a.ToString());
		}

		[TestMethod]
		public void FromMinimalDate()
		{
			var time = OscTimeTag.FromDateTime(OscTimeTag.MinDateTime);
			var actual = time.Value;
			Assert.AreEqual(0u, actual);
		}

		[TestMethod]
		public void FromSmallTime()
		{
			var time = OscTimeTag.FromDateTime(OscTimeTag.MinDateTime.AddMilliseconds(1234.56));
			var actual = time.Value;
			Assert.AreEqual(5304284610u, actual);
		}

		[TestMethod]
		public void FromTimespan()
		{
			var span = new TimeSpan(0, 0, 0, 1, 234);
			var t = OscTimeTag.FromTimeSpan(span);
			Assert.AreEqual(5299989643u, t.Value);
			Assert.AreEqual(1234, t.ToMilliseconds());
		}

		[TestMethod]
		public void FromValue()
		{
			var values = new List<(ulong, string)>
			{
				(16280353193693544448, "2020-02-13T06:45:13.0000000Z"),
				(16163728278807480631, "2019-04-05T00:00:59.0010000Z"),
				(16163728278846135336, "2019-04-05T00:00:59.0100000Z"),
				(16163728278803185664, "2019-04-05T00:00:59.0000000Z"),
				(16163728025400115200, "2019-04-05T00:00:00.0000000Z"),
				(16181735293039820143, "2019-05-23T12:37:23.7150000Z")
			};

			foreach (var e in values)
			{
				var actual = new OscTimeTag(e.Item1);
				actual.Value.Dump();
				Assert.AreEqual(e.Item1, actual.Value);
				Assert.AreEqual(e.Item2, actual.ToString());
			}
		}

		[TestMethod]
		public void GetHashCodeShouldSucceed()
		{
			Assert.AreEqual(0, new OscTimeTag().GetHashCode());
			Assert.AreEqual(0, OscTimeTag.MinValue.GetHashCode());
			Assert.AreEqual(1895321856, new OscTimeTag(new DateTime(2020, 02, 14, 04, 35, 12, DateTimeKind.Utc)).GetHashCode());
			Assert.AreEqual(1878481506, new OscTimeTag(16136033268821655552).GetHashCode());
			Assert.AreEqual(2147483647, OscTimeTag.MaxValue.GetHashCode());
		}

		[TestMethod]
		public void MaxValue()
		{
			var expected = new OscTimeTag(0xffffffffffffffff);
			Assert.AreEqual(expected, OscTimeTag.MaxValue);
		}

		[TestMethod]
		public void MinPreciseValue()
		{
			var expected1 = new OscTimeTag(1);
			Assert.AreEqual(0.0000000002328306437080797375m, expected1.PreciseValue);
			Assert.AreEqual(1m, expected1.Value);
			Assert.AreEqual(599266080000000000, expected1.ToDateTime().Ticks);

			var expected2 = new OscTimeTag(2);
			Assert.AreEqual(0.0000000004656612874161594751m, expected2.PreciseValue);
			Assert.AreEqual(2u, expected2.Value);
			Assert.AreEqual(599266080000000000, expected2.ToDateTime().Ticks);

			var expected = expected2 - expected1;
			Assert.AreEqual(0, expected.Ticks);
		}

		[TestMethod]
		public void MinValue()
		{
			var expected = new OscTimeTag(0);
			Assert.AreEqual(expected, OscTimeTag.MinValue);
		}

		[TestMethod]
		public void ParseTime()
		{
			var values = new List<(string, ulong, string)>
			{
				("2019-04-05T00:00:59.1Z", 16163728279232682393, "2019-04-05T00:00:59.1000000Z"),
				("2019-04-05T00:00:59.01Z", 16163728278846135336, "2019-04-05T00:00:59.0100000Z"),
				("2019-04-05T00:00:59.001Z", 16163728278807480631, "2019-04-05T00:00:59.0010000Z"),
				("2019-04-05T00:00:59.0010Z", 16163728278807480631, "2019-04-05T00:00:59.0010000Z"),
				("2019-04-05T00:00:59.7500Z", 16163728282024411135, "2019-04-05T00:00:59.7500000Z"),
				("2019-04-05T00:00:59Z", 16163728278803185664, "2019-04-05T00:00:59.0000000Z"),
				("2019-04-05", 16163728025400115200, "2019-04-05T00:00:00.0000000Z"),
				("2019-05-23T12:37:23.7150Z", 16181735293039820143, "2019-05-23T12:37:23.7150000Z")
			};

			foreach (var e in values)
			{
				var actual = OscTimeTag.Parse(e.Item1);
				actual.Value.Dump();
				Assert.AreEqual(e.Item2, actual.Value);
				Assert.AreEqual(e.Item3, actual.ToString());

				var actual2 = OscTimeTag.Parse(e.Item3);
				Assert.AreEqual(actual, actual2);
			}
		}

		/// <summary>
		/// NTP can only handle ticks in 10k seconds when you break the 10.5k value the value
		/// will wrap to 20k which means it will round to 2 ms.
		/// </summary>
		[TestMethod]
		public void PrecisionTests()
		{
			var value = OscTimeTag.MinValue;
			Assert.AreEqual(0, value.PreciseValue);

			value = OscTimeTag.FromTimeSpan(TimeSpan.Zero);
			Assert.AreEqual(OscTimeTag.MinValue, value);

			value = OscTimeTag.FromTicks(TimeSpan.FromMilliseconds(123).Ticks);
			Assert.AreEqual(123, value.ToMilliseconds());
			Assert.AreEqual(123, value.ToTimeSpan().TotalMilliseconds);

			value = OscTimeTag.FromMilliseconds(123);
			Assert.AreEqual(123, value.ToMilliseconds());
			Assert.AreEqual(123, value.ToTimeSpan().TotalMilliseconds);

			value = OscTimeTag.FromTicks(10000);
			Assert.AreEqual(1, value.ToMilliseconds());
			Assert.AreEqual(10000, value.ToTimeSpan().Ticks);
			Assert.AreEqual(1, value.ToTimeSpan().TotalMilliseconds);

			value = OscTimeTag.FromTicks(5000);
			Assert.AreEqual(5000, value.ToTimeSpan().Ticks);
			Assert.AreEqual(0.5, value.ToTimeSpan().TotalMilliseconds);
			value = OscTimeTag.FromTicks(5001);
			Assert.AreEqual(5001, value.ToTimeSpan().Ticks);
			Assert.AreEqual(0.5001, value.ToTimeSpan().TotalMilliseconds);

			value = OscTimeTag.FromTicks(15000);
			Assert.AreEqual(1.5, value.ToMilliseconds());
			Assert.AreEqual(15000, value.ToTimeSpan().Ticks);
			value = OscTimeTag.FromTicks(15001);
			Assert.AreEqual(1.5001, value.ToMilliseconds());
			Assert.AreEqual(15001, value.ToTimeSpan().Ticks);
			Assert.AreEqual(1.5001, value.ToTimeSpan().TotalMilliseconds);
		}

		[TestMethod]
		public void Subtract()
		{
			var expected = new DateTime(2019, 1, 20, 08, 50, 12, DateTimeKind.Utc);
			var span = TimeSpan.FromMilliseconds(123);
			var t1 = new OscTimeTag(expected);
			var t2 = new OscTimeTag(expected.Add(span));
			var actual = t2 - t1;
			Assert.AreEqual(123, actual.TotalMilliseconds);
			Assert.AreEqual(t1, t2 - span);
		}

		[TestMethod]
		public void ToFromDateTime()
		{
			var dateTime1 = "2022-08-03T18:27:12.5619757Z".ToUtcDateTime();
			var oscTime = OscTimeTag.FromDateTime(dateTime1);
			var dateTime2 = oscTime.ToDateTime();

			dateTime1.ToString("O").Dump();
			dateTime2.ToString("O").Dump();

			Assert.AreEqual(dateTime1, dateTime2);
		}

		[TestMethod]
		public void ToFromUtcNowUsingParse()
		{
			var time = OscTimeTag.UtcNow;
			var text = time.ToString();
			var time2 = OscTimeTag.Parse(text);

			time.Dump();
			time2.Dump();
			Assert.AreEqual(time.ToString(), time2.ToString());

			time = OscTimeTag.Now;
			text = time.ToString();
			time2 = OscTimeTag.Parse(text);

			time.Dump();
			time2.Dump();
			Assert.AreEqual(time.ToString(), time2.ToString());
		}

		[TestMethod]
		public void ToMillisecond()
		{
			var t = OscTimeTag.FromMilliseconds(1234f);
			Assert.AreEqual(1234, t.ToMilliseconds());
			Assert.AreEqual(5299989643u, t.Value);
			Assert.AreEqual("1900-01-01T00:00:01.2340000Z", t.ToString());

			t = OscTimeTag.FromMilliseconds(1000f);
			Assert.AreEqual(1000, t.ToMilliseconds());
			Assert.AreEqual(4294967296u, t.Value);
			Assert.AreEqual("1900-01-01T00:00:01.0000000Z", t.ToString());

			Assert.AreEqual(1000, t.ToMilliseconds());
			Assert.AreEqual(4294967296u, t.Value);
			Assert.AreEqual("1900-01-01T00:00:01.0000000Z", t.ToString());

			t = OscTimeTag.FromMilliseconds(1001f);
			Assert.AreEqual(1001, t.ToMilliseconds());
			Assert.AreEqual(4299262263u, t.Value);
			Assert.AreEqual("1900-01-01T00:00:01.0010000Z", t.ToString());
		}

		[TestMethod]
		public void ToMinimalDate()
		{
			var actual = new OscTimeTag(0);
			Assert.AreEqual(OscTimeTag.MinValue, actual);
			Assert.AreEqual(OscTimeTag.MinDateTime, actual.ToDateTime());
		}

		[TestMethod]
		public void ToSmallTime()
		{
			var time = new OscTimeTag(5304284610u);
			time.Value.Dump();
			Assert.AreEqual(1235, time.ToMilliseconds());
		}

		#endregion
	}
}