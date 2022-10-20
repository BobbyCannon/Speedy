#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Devices.Location;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class LocationExtensionsTests : SpeedyTest
{
	#region Methods

	[TestMethod]
	public void GetEllipsoidValue()
	{
		var scenarios = new List<(IVerticalLocation location, IVerticalLocation relativeTo, double expected)>
		{
			// 10 Terrain above 100 Ellipsoid should be 110
			(new VerticalLocation(10, AltitudeReferenceType.Terrain),
				new VerticalLocation(100, AltitudeReferenceType.Ellipsoid),
				110),
			// 10 Ellipsoid should just be 10
			(new VerticalLocation(10, AltitudeReferenceType.Ellipsoid),
				new VerticalLocation(100, AltitudeReferenceType.Ellipsoid),
				10),
			// Terrain above another Terrain should just be the original Terrain
			(new VerticalLocation(12, AltitudeReferenceType.Ellipsoid),
				new VerticalLocation(24, AltitudeReferenceType.Terrain),
				12)
		};

		foreach (var x in scenarios)
		{
			var actual = x.location.GetEllipsoidAltitude(x.relativeTo);
			Assert.AreEqual(x.expected, actual);
		}
	}

	#endregion
}