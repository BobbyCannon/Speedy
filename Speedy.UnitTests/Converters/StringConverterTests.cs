#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Converters;

#endregion

namespace Speedy.UnitTests.Converters
{
	[TestClass]
	public class StringConverterTests
	{
		#region Methods

		[TestMethod]
		public void ImplicitOperator()
		{
			char charValue = new StringConverter<char>(" ");
			Assert.AreEqual(' ', charValue);

			string charString = new StringConverter<char>(" ");
			Assert.AreEqual(" ", charString);

			StringConverter<char> charConverter = charValue;
			Assert.AreEqual(typeof(StringConverter<char>), charConverter.GetType());
			Assert.AreEqual(" ", charConverter.TargetString);
			Assert.AreEqual(" ", charConverter.ToString());

			charConverter = charString;
			Assert.AreEqual(typeof(StringConverter<char>), charConverter.GetType());
			Assert.AreEqual(" ", charConverter.TargetString);
			Assert.AreEqual(" ", charConverter.ToString());

			bool boolValue = new StringConverter<bool>("true");
			Assert.AreEqual(true, boolValue);

			string boolString = new StringConverter<bool>("true");
			Assert.AreEqual("true", boolString);

			StringConverter<bool> boolConverter = boolValue;
			Assert.AreEqual(typeof(StringConverter<bool>), boolConverter.GetType());
			Assert.AreEqual("True", boolConverter.TargetString);
			Assert.AreEqual("True", boolConverter.ToString());

			boolConverter = boolString;
			Assert.AreEqual(typeof(StringConverter<bool>), boolConverter.GetType());
			Assert.AreEqual("true", boolConverter.TargetString);
			Assert.AreEqual("true", boolConverter.ToString());

			Version versionValue = new StringConverter<Version>("1.2.3.4");
			Assert.AreEqual(true, boolValue);

			string versionString = new StringConverter<Version>("1.2.3.4");
			Assert.AreEqual("1.2.3.4", versionString);

			StringConverter<Version> versionConverter = versionValue;
			Assert.AreEqual(typeof(StringConverter<Version>), versionConverter.GetType());
			Assert.AreEqual("1.2.3.4", versionConverter.TargetString);

			versionConverter = versionString;
			Assert.AreEqual(typeof(StringConverter<Version>), versionConverter.GetType());
			Assert.AreEqual("1.2.3.4", versionConverter.TargetString);
		}

		[TestMethod]
		public void ShouldEqual()
		{
			var scenarios = new (string value, Type targetType, object expected)[]
			{
				new ValueTuple<string, Type, object>(null, typeof(char?), null),
				new ValueTuple<string, Type, object>("\uFFFF", typeof(char), char.MaxValue),
				new ValueTuple<string, Type, object>("\0", typeof(char), char.MinValue),
				new ValueTuple<string, Type, object>(null, typeof(string), null),
				new ValueTuple<string, Type, object>("", typeof(string), string.Empty),
				new ValueTuple<string, Type, object>("abc123", typeof(string), "abc123"),
				new ValueTuple<string, Type, object>("123456", typeof(int), 123456),
				new ValueTuple<string, Type, object>("2147483647", typeof(int), int.MaxValue),
				new ValueTuple<string, Type, object>("-2147483648", typeof(int), int.MinValue),
				new ValueTuple<string, Type, object>("9223372036854775807", typeof(nint), nint.MaxValue),
				new ValueTuple<string, Type, object>("-9223372036854775808", typeof(nint), nint.MinValue),
				new ValueTuple<string, Type, object>("4294967295", typeof(uint), uint.MaxValue),
				new ValueTuple<string, Type, object>("0", typeof(uint), uint.MinValue),
				new ValueTuple<string, Type, object>("18446744073709551615", typeof(nuint), nuint.MaxValue),
				new ValueTuple<string, Type, object>("0", typeof(nuint), nuint.MinValue),
				new ValueTuple<string, Type, object>("255", typeof(byte), byte.MaxValue),
				new ValueTuple<string, Type, object>("0", typeof(byte), byte.MinValue),
				new ValueTuple<string, Type, object>("9223372036854775807", typeof(long), long.MaxValue),
				new ValueTuple<string, Type, object>("-9223372036854775808", typeof(long), long.MinValue),
				new ValueTuple<string, Type, object>("18446744073709551615", typeof(ulong), ulong.MaxValue),
				new ValueTuple<string, Type, object>("0", typeof(ulong), ulong.MinValue),
				new ValueTuple<string, Type, object>(null, typeof(DateTime?), null),
				new ValueTuple<string, Type, object>("2022-04-06", typeof(DateTime), new DateTime(2022, 04, 06)),
				new ValueTuple<string, Type, object>("2022/04/06", typeof(DateTime), new DateTime(2022, 04, 06)),
				new ValueTuple<string, Type, object>("04-06-2022", typeof(DateTime), new DateTime(2022, 04, 06)),
				new ValueTuple<string, Type, object>("04/06/2022", typeof(DateTime), new DateTime(2022, 04, 06)),
				new ValueTuple<string, Type, object>("9999-12-31T23:59:59.9999999", typeof(DateTime), DateTime.MaxValue),
				new ValueTuple<string, Type, object>("0001-01-01T00:00:00.0000000", typeof(DateTime), DateTime.MinValue),
				new ValueTuple<string, Type, object>("http://domain.com", typeof(Uri), new Uri("http://domain.com")),
				new ValueTuple<string, Type, object>("ftp://domain", typeof(Uri), new Uri("ftp://domain")),
				new ValueTuple<string, Type, object>("TRUE", typeof(bool), true),
				new ValueTuple<string, Type, object>("True", typeof(bool), true),
				new ValueTuple<string, Type, object>("true", typeof(bool), true),
				new ValueTuple<string, Type, object>("FALSE", typeof(bool), false),
				new ValueTuple<string, Type, object>("False", typeof(bool), false),
				new ValueTuple<string, Type, object>("false", typeof(bool), false),
				new ValueTuple<string, Type, object>("1.23456", typeof(float), 1.23456f),
				new ValueTuple<string, Type, object>("3.4028235E+38", typeof(float), float.MaxValue),
				new ValueTuple<string, Type, object>("-3.4028235E+38", typeof(float), float.MinValue),
				new ValueTuple<string, Type, object>("1.23456", typeof(double), 1.23456d),
				new ValueTuple<string, Type, object>("1.7976931348623157E+308", typeof(double), double.MaxValue),
				new ValueTuple<string, Type, object>("-1.7976931348623157E+308", typeof(double), double.MinValue),
				new ValueTuple<string, Type, object>("1.23456", typeof(decimal), 1.23456m),
				new ValueTuple<string, Type, object>("79228162514264337593543950335", typeof(decimal), decimal.MaxValue),
				new ValueTuple<string, Type, object>("-79228162514264337593543950335", typeof(decimal), decimal.MinValue),
				new ValueTuple<string, Type, object>("65535", typeof(ushort), ushort.MaxValue),
				new ValueTuple<string, Type, object>("0", typeof(ushort), ushort.MinValue),
				new ValueTuple<string, Type, object>("32767", typeof(short), short.MaxValue),
				new ValueTuple<string, Type, object>("-32768", typeof(short), short.MinValue),
				new ValueTuple<string, Type, object>("1.0", typeof(Version), new Version(1, 0)),
				new ValueTuple<string, Type, object>("1.2.3", typeof(Version), new Version(1, 2, 3)),
				new ValueTuple<string, Type, object>("1.2.3.4", typeof(Version), new Version(1, 2, 3, 4))
			};

			foreach (var scenario in scenarios)
			{
				var actual = StringConverter.Parse(scenario.targetType, scenario.value);
				Assert.AreEqual(scenario.expected, actual, $"{scenario.value} != {scenario.expected}");

				var result = StringConverter.TryParse(scenario.targetType, scenario.value, out actual);
				Assert.IsTrue(result, "Failed to try parse.");
				Assert.AreEqual(scenario.expected, actual, $"{scenario.value} != {scenario.expected}");

				var converter = StringConverter.Create(scenario.targetType, scenario.value);
				result = converter.TryParse(scenario.value, out actual);
				Assert.IsTrue(result, "Failed to try parse.");
				Assert.AreEqual(scenario.expected, actual, $"{scenario.value} != {scenario.expected}");

				actual = converter.Parse();
				Assert.AreEqual(scenario.expected, actual, $"{scenario.value} != {scenario.expected}");
				
				actual = converter.Parse(scenario.value);
				Assert.AreEqual(scenario.expected, actual, $"{scenario.value} != {scenario.expected}");
			}
		}

		#endregion
	}
}