#region References

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Extensions;
using Speedy.Serialization.Converters;

#endregion

namespace Speedy.UnitTests.Serialization.Converters
{
	[TestClass]
	public class IsoDateTimeConverterTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void ReadJson()
		{
			var scenarios = new Dictionary<string, IsoDateTime>
			{
				{
					"\"2022-08-22T04:00:00-04:00/P8DT6H\"",
					NewIsoDateTime("2022-08-22T08:00:00.0000000Z", "8.06:00:00")
				},
				{
					"\"2022-08-22T00:00:00/P1Y2M8DT6H4M12.456S\"",
					NewIsoDateTime("2022-08-22T00:00:00Z", "434.06:04:12.456")
				},
				{
					"\"2022-08-22T12:34:56/PT6H4M12.456S\"",
					NewIsoDateTime("2022-08-22T12:34:56Z", "06:04:12.456")
				}
			};

			foreach (var scenario in scenarios)
			{
				var converter = new IsoDateTimeConverter();
				var textReader = new StringReader(scenario.Key);
				var reader = new JsonTextReader(textReader);
				reader.Read();
				var actual = converter.ReadJson(reader, typeof(IsoDateTime), default, false, null);
				Assert.AreEqual(scenario.Value, actual);
			}
		}

		private IsoDateTime NewIsoDateTime(string datetime, string duration)
		{
			return new IsoDateTime
			{
				DateTime = datetime.ToUtcDateTime(),
				Duration = TimeSpan.Parse(duration)
			};
		}

		#endregion
	}
}