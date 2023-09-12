#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Serialization;
using Speedy.UnitTests.Factories;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class SerializationExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void IsJson()
	{
		var testCases = new[] { "\"\"", "[]", "{}", "{\"aoeu\":1}" };

		foreach (var testCase in testCases)
		{
			Assert.IsTrue(testCase.IsJson(), $"{testCase} is not detected as valid JSON but should be.");
		}

		// Invalid JSON
		testCases = new[] { "[aoeu:1]", "[\"aoeu\":aoeu]" };

		foreach (var testCase in testCases)
		{
			Assert.IsFalse(testCase.IsJson(), $"{testCase} is detected as valid JSON but should not be.");
		}
	}

	[TestMethod]
	public void ToJsonCamelCaseParameter()
	{
		SetTime(new DateTime(2019, 07, 17, 20, 05, 55, DateTimeKind.Utc));
		var logEvent = EntityFactory.GetLogEvent("Hello", LogLevel.Critical, x => x.SyncId = Guid.Parse("51387F23-C0CE-47B6-BFAD-5E273B82A5A1"));

		// First use the default values
		var expected = "{\"$id\":\"1\",\"AcknowledgedOn\":null,\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":0,\"IsDeleted\":false,\"Level\":0,\"LoggedOn\":\"2019-07-17T20:05:55Z\",\"Message\":\"Hello\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\",\"SyncId\":\"51387f23-c0ce-47b6-bfad-5e273b82a5a1\"}";
		var actual = logEvent.ToJson();
		//actual.Escape().CopyToClipboard().Dump();
		Assert.AreEqual(expected, actual);

		// Now override the default
		expected = "{\"$id\":\"1\",\"acknowledgedOn\":null,\"createdOn\":\"2019-07-17T20:05:55Z\",\"id\":0,\"isDeleted\":false,\"level\":0,\"loggedOn\":\"2019-07-17T20:05:55Z\",\"message\":\"Hello\",\"modifiedOn\":\"2019-07-17T20:05:55Z\",\"syncId\":\"51387f23-c0ce-47b6-bfad-5e273b82a5a1\"}";
		actual = logEvent.ToJson(camelCase: true);
		//actual.Escape().CopyToClipboard().Dump();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void ToJsonConvertEnumToStringParameter()
	{
		SetTime(new DateTime(2019, 07, 17, 20, 05, 55, DateTimeKind.Utc));
		var logEvent = EntityFactory.GetLogEvent("Hello", LogLevel.Debug, x => x.SyncId = Guid.Parse("3EC4021A-02C9-4A03-9314-6C078F1A5596"));

		// First use the default values
		var expected = "{\"$id\":\"1\",\"AcknowledgedOn\":null,\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":0,\"IsDeleted\":false,\"Level\":4,\"LoggedOn\":\"2019-07-17T20:05:55Z\",\"Message\":\"Hello\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\",\"SyncId\":\"3ec4021a-02c9-4a03-9314-6c078f1a5596\"}";
		var actual = logEvent.ToJson();
		//actual.Escape().CopyToClipboard().Dump();
		Assert.AreEqual(expected, actual);

		// Now override the default
		expected = "{\"$id\":\"1\",\"AcknowledgedOn\":null,\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":0,\"IsDeleted\":false,\"Level\":\"Debug\",\"LoggedOn\":\"2019-07-17T20:05:55Z\",\"Message\":\"Hello\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\",\"SyncId\":\"3ec4021a-02c9-4a03-9314-6c078f1a5596\"}";
		actual = logEvent.ToJson(convertEnumsToString: true);
		//actual.Escape().CopyToClipboard().Dump();
		Assert.AreEqual(expected, actual);

		// Now override the default and use camel casing
		expected = "{\"$id\":\"1\",\"acknowledgedOn\":null,\"createdOn\":\"2019-07-17T20:05:55Z\",\"id\":0,\"isDeleted\":false,\"level\":\"debug\",\"loggedOn\":\"2019-07-17T20:05:55Z\",\"message\":\"Hello\",\"modifiedOn\":\"2019-07-17T20:05:55Z\",\"syncId\":\"3ec4021a-02c9-4a03-9314-6c078f1a5596\"}";
		actual = logEvent.ToJson(camelCase: true, convertEnumsToString: true);
		//actual.Escape().CopyToClipboard().Dump();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void ToJsonIgnoreNullParameter()
	{
		SetTime(new DateTime(2019, 07, 17, 20, 05, 55, DateTimeKind.Utc));
		var logEvent = EntityFactory.GetLogEvent(null, LogLevel.Error, x => x.SyncId = Guid.Parse("B2BCD532-E952-4966-A6F0-09A14C6C6DDA"));

		// First use the default values
		var expected = "{\"$id\":\"1\",\"AcknowledgedOn\":null,\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":0,\"IsDeleted\":false,\"Level\":1,\"LoggedOn\":\"2019-07-17T20:05:55Z\",\"Message\":null,\"ModifiedOn\":\"2019-07-17T20:05:55Z\",\"SyncId\":\"b2bcd532-e952-4966-a6f0-09a14c6c6dda\"}";
		var actual = logEvent.ToJson();
		//actual.Escape().CopyToClipboard().Dump();
		Assert.AreEqual(expected, actual);

		// Now override the default
		expected = "{\"$id\":\"1\",\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":0,\"IsDeleted\":false,\"Level\":1,\"LoggedOn\":\"2019-07-17T20:05:55Z\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\",\"SyncId\":\"b2bcd532-e952-4966-a6f0-09a14c6c6dda\"}";
		actual = logEvent.ToJson(ignoreNullValues: true);
		//actual.Escape().CopyToClipboard().Dump();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void ToJsonIndentParameter()
	{
		var test = new { Name = "John", Age = 21 };
		var expected = "{\"$id\":\"1\",\"Age\":21,\"Name\":\"John\"}";
		var actual = test.ToJson();
		Assert.AreEqual(expected, actual);

		expected = "{\r\n  \"$id\": \"1\",\r\n  \"Age\": 21,\r\n  \"Name\": \"John\"\r\n}";
		actual = test.ToJson(true);
		Assert.AreEqual(expected, actual);
	}

	#endregion
}