#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class JsonTests
	{
		#region Methods

		[TestMethod]
		public void DeserializeByteArray()
		{
			var input = "[0,1,2]";
			var expected = new byte[] { 0, 1, 2 };
			var actual = input.FromJson<byte[]>();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DeserializeDateTimeSpan()
		{
			var input = "[\"23:59:59.9990000\",\"12:59:59\",\"21:11:37.3400373\"]";

			var expected = new[]
			{
				new TimeSpan(0, 23, 59, 59, 999),
				new TimeSpan(12, 59, 59),
				new TimeSpan(762973400373)
			};

			var actual = input.FromJson<TimeSpan[]>();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DeserializeDecimalArray()
		{
			var input = "[0.0,1.0,2.0]";
			var expected = new[] { 0.0m, 1.0m, 2.0m };
			var actual = input.FromJson<decimal[]>();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DeserializeIntegerArray()
		{
			var input = "[0,1,2]";
			var expected = new[] { 0, 1, 2 };
			var actual = input.FromJson<int[]>();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DeserializeSByteArray()
		{
			var input = "[-2,-1,0,1,2]";
			var expected = new sbyte[] { -2, -1, 0, 1, 2 };
			var actual = input.FromJson<sbyte[]>();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DeserializeShortArray()
		{
			var input = "[0,1,2]";
			var expected = new short[] { 0, 1, 2 };
			var actual = input.FromJson<short[]>();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializeDateTimeArray()
		{
			var input = new[]
			{
				new DateTime(2016, 01, 01, 01, 23, 45, 999, DateTimeKind.Utc),
				new DateTime(2015, 12, 30, 01, 23, 45, 001, DateTimeKind.Local)
			};

			var expected = "[\"2016-01-01T01:23:45.999Z\",\"2015-12-30T01:23:45.001\"]";
			var actual = input.ToJson();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DeserializeDateTimeArray()
		{
			var input = "[\"2016-01-01T01:23:45.999Z\",\"2015-12-30T01:23:45.001\"]";

			var expected = new[]
			{
				new DateTime(2016, 01, 01, 01, 23, 45, 999, DateTimeKind.Utc),
				new DateTime(2015, 12, 30, 01, 23, 45, 001, DateTimeKind.Local)
			};

			var actual = input.FromJson<DateTime[]>();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializeDateTimeSpan()
		{
			var input = new[]
			{
				new TimeSpan(0, 23, 59, 59, 999),
				new TimeSpan(12, 59, 59),
				new TimeSpan(762973400373)
			};

			var expected = "[\"23:59:59.9990000\",\"12:59:59\",\"21:11:37.3400373\"]";
			var actual = input.ToJson();
			TestHelper.AreEqual(expected, actual);
		}

		#endregion
	}
}