using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.Location;
using SpeedyLocation = Speedy.Data.Location.Location;

namespace Speedy.UnitTests.Data.Location;

[TestClass]
public class LocationComparerTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void OnlyHorizontalShouldUpdate()
	{
		TestHelper.SetTime(new DateTime(2022, 11, 25, 01, 58, 12, DateTimeKind.Utc));

		var comparer = new LocationComparer<SpeedyLocation, IHorizontalLocation, IVerticalLocation>();
		var location = new SpeedyLocation();
		var update = new SpeedyLocation
		{
			HorizontalLocation =
			{
				SourceName = "test"
			},
			VerticalLocation =
			{
				SourceName = "test"
			}
		};

		AreEqual(false, comparer.ShouldUpdate(location, update));

		update.HorizontalLocation.Latitude = 12.345;
		update.HorizontalLocation.Longitude = 80.543;
		AreEqual(false, comparer.ShouldUpdate(location, update));

		update.HorizontalLocation.HasValue = true;
		AreEqual(true, comparer.ShouldUpdate(location, update));
		
		update.HorizontalLocation.StatusTime = TimeService.UtcNow;
		AreEqual(true, comparer.ShouldUpdate(location, update));
		AreEqual(true, comparer.UpdateWith(ref location, update));

		AreEqual(TimeService.UtcNow, location.HorizontalLocation.StatusTime);
		AreEqual("test", location.HorizontalLocation.SourceName);
		AreNotEqual(TimeService.UtcNow, location.VerticalLocation.StatusTime);
		AreEqual(null, location.VerticalLocation.SourceName);
	}
	
	[TestMethod]
	public void OnlyVerticalShouldUpdate()
	{
		TestHelper.SetTime(new DateTime(2022, 11, 25, 01, 58, 12, DateTimeKind.Utc));

		var comparer = new LocationComparer<SpeedyLocation, IHorizontalLocation, IVerticalLocation>();
		var location = new SpeedyLocation();
		var update = new SpeedyLocation
		{
			HorizontalLocation =
			{
				SourceName = "test"
			},
			VerticalLocation =
			{
				SourceName = "test"
			}
		};

		AreEqual(false, comparer.ShouldUpdate(location, update));

		update.VerticalLocation.Altitude = 123.45;
		update.VerticalLocation.AltitudeReference = AltitudeReferenceType.Ellipsoid;
		AreEqual(false, comparer.ShouldUpdate(location, update));

		update.VerticalLocation.HasValue = true;
		AreEqual(true, comparer.ShouldUpdate(location, update));
		
		update.VerticalLocation.StatusTime = TimeService.UtcNow;
		AreEqual(true, comparer.ShouldUpdate(location, update));
		AreEqual(true, comparer.UpdateWith(ref location, update));

		AreNotEqual(TimeService.UtcNow, location.HorizontalLocation.StatusTime);
		AreEqual(null, location.HorizontalLocation.SourceName);
		AreEqual(TimeService.UtcNow, location.VerticalLocation.StatusTime);
		AreEqual("test", location.VerticalLocation.SourceName);
	}

	#endregion
}