#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Serialization.Converters;

#endregion

namespace Speedy.UnitTests.Serialization.Converters
{
	[TestClass]
	public class VersionStringConverterTests
	{
		#region Methods

		[TestMethod]
		public void SerializationNotUsingConverter()
		{
			var version = new Version(1, 2, 3, 4);
			var expected = "\"1.2.3.4\"";
			var actual = version.ToJson();
			actual.Dump();
			Assert.AreEqual(expected, actual);

			var actualVersion = actual.FromJson<Version>();
			Assert.AreEqual(version, actualVersion);
		}

		[TestMethod]
		public void SerializationUsingConverter()
		{
			var settings = new SerializerSettings();
			settings.JsonSettings.Converters.Add(new VersionStringConverter());
			var version = new Version(1, 2, 3, 4);
			var expected = "{\"Major\":1,\"Minor\":2,\"Build\":3,\"Revision\":4}";
			var actual = version.ToJson(settings);
			//actual.Escape().CopyToClipboard().Dump();
			Assert.AreEqual(expected, actual);

			var actualVersion = actual.FromJson<Version>(settings);
			Assert.AreEqual(version, actualVersion);
			
			settings = new SerializerSettings();
			expected = "\"1.2.3.4\"";
			actual = version.ToJson(settings);
			//actual.Escape().CopyToClipboard().Dump();
			Assert.AreEqual(expected, actual);

			actualVersion = actual.FromJson<Version>(settings);
			Assert.AreEqual(version, actualVersion);
		}
		
		[TestMethod]
		public void SerializationUsingConverterWithCamelCase()
		{
			var settings = new SerializerSettings(camelCase: true);
			settings.JsonSettings.Converters.Add(new VersionStringConverter());
			var version = new Version(1, 2, 3, 4);
			var expected = "{\"major\":1,\"minor\":2,\"build\":3,\"revision\":4}";
			
			var actual = version.ToJson(settings);
			//actual.Escape().CopyToClipboard().Dump();
			Assert.AreEqual(expected, actual);

			var actualVersion = actual.FromJson<Version>(settings);
			Assert.AreEqual(version, actualVersion);
		}

		#endregion
	}
}