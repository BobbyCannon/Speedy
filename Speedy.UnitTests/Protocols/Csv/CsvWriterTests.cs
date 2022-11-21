#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;
using Speedy.Devices.Location;
using Speedy.Protocols.Csv;

#endregion

namespace Speedy.UnitTests.Protocols.Csv;

[TestClass]
public class CsvWriterTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void ShouldWrite()
	{
		var location = new VerticalLocation
		{
			Accuracy = 1.2,
			AccuracyReference = AccuracyReferenceType.Meters,
			Altitude = 123.45,
			AltitudeReference = AltitudeReferenceType.Ellipsoid,
			Flags = LocationFlags.All,
			ProviderName = "Provider 1",
			SourceName = "The Source",
			StatusTime = new DateTime(2022, 11, 21, 12, 16, 15, DateTimeKind.Utc)
		};

		var history = new BaseObservableCollection<VerticalLocation>();
		location.Flags = LocationFlags.HasLocation | LocationFlags.HasSpeed;
		location.StatusTime += TimeSpan.FromSeconds(1);
		history.Add((VerticalLocation) location.ShallowClone());

		location.Flags = LocationFlags.All;
		location.StatusTime += TimeSpan.FromSeconds(1);
		history.Add((VerticalLocation) location.ShallowClone());

		var actual = CsvWriter.Write(history);
		actual.Dump();

		var expected = "Accuracy,AccuracyReference,Altitude,AltitudeReference,Flags,HasAccuracy,HasChanges,HasHeading,HasSpeed,HasValue,Heading,InformationId,ProviderName,SourceName,Speed,StatusTime\r\n1.2,1,123.45,2,6,True,True,False,True,True,0,2ab3b4a0-a387-409a-bba3-b74f75972463,Provider 1,The Source,0,2022-11-21T12:16:16.0000000Z\r\n1.2,1,123.45,2,7,True,True,True,True,True,0,2ab3b4a0-a387-409a-bba3-b74f75972463,Provider 1,The Source,0,2022-11-21T12:16:17.0000000Z";
		AreEqual(expected, actual);
	}

	#endregion
}