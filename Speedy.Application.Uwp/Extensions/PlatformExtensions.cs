#region References

using Windows.Devices.Geolocation;
using Speedy.Data.Location;

#endregion

namespace Speedy.Application.Uwp.Extensions;

public static class PlatformExtensions
{
	#region Methods

	/// <summary>
	/// Convert altitude reference type to system.
	/// </summary>
	public static AltitudeReferenceSystem ToAltitudeReferenceSystem(this AltitudeReferenceType altitudeReference)
	{
		return altitudeReference switch
		{
			AltitudeReferenceType.Terrain => AltitudeReferenceSystem.Terrain,
			AltitudeReferenceType.Ellipsoid => AltitudeReferenceSystem.Ellipsoid,
			AltitudeReferenceType.Geoid => AltitudeReferenceSystem.Geoid,
			AltitudeReferenceType.Unspecified => AltitudeReferenceSystem.Unspecified,
			_ => AltitudeReferenceSystem.Unspecified
		};
	}

	/// <summary>
	/// Convert altitude reference system to type.
	/// </summary>
	public static AltitudeReferenceType ToAltitudeReferenceType(this AltitudeReferenceSystem altitudeReference)
	{
		return altitudeReference switch
		{
			AltitudeReferenceSystem.Terrain => AltitudeReferenceType.Terrain,
			AltitudeReferenceSystem.Ellipsoid => AltitudeReferenceType.Ellipsoid,
			AltitudeReferenceSystem.Geoid => AltitudeReferenceType.Geoid,
			AltitudeReferenceSystem.Unspecified => AltitudeReferenceType.Unspecified,
			_ => AltitudeReferenceType.Unspecified
		};
	}

	public static BasicGeoposition ToBasicGeoposition(this IBasicLocation location, double? altitudeOverride = null)
	{
		if ((altitudeOverride == null)
			&& (location.AltitudeReference == AltitudeReferenceType.Unspecified))
		{
			altitudeOverride = 0;
		}

		return new BasicGeoposition
		{
			Altitude = altitudeOverride ?? location.Altitude,
			Latitude = location.Latitude,
			Longitude = location.Longitude
		};
	}
	
	public static BasicGeoposition ToBasicGeoposition(this ILocation<IHorizontalLocation, IVerticalLocation> location, double? altitudeOverride = null)
	{
		if ((altitudeOverride == null)
			&& (location.VerticalLocation.AltitudeReference == AltitudeReferenceType.Unspecified))
		{
			altitudeOverride = 0;
		}

		return new BasicGeoposition
		{
			Altitude = altitudeOverride ?? location.VerticalLocation.Altitude,
			Latitude = location.HorizontalLocation.Latitude,
			Longitude = location.HorizontalLocation.Longitude
		};
	}
	
	public static BasicGeoposition ToBasicGeoposition(this IMinimalHorizontalLocation location, double? altitudeOverride = null)
	{
		return new BasicGeoposition
		{
			Altitude = altitudeOverride ?? 0,
			Latitude = location.Latitude,
			Longitude = location.Longitude
		};
	}

	public static Geopoint ToGeopoint(this IBasicLocation location)
	{
		var reference = location.AltitudeReference.ToAltitudeReferenceSystem();

		if ((reference == AltitudeReferenceSystem.Unspecified)
			|| (reference == AltitudeReferenceSystem.Terrain))
		{
			return new Geopoint(location.ToBasicGeoposition(0), AltitudeReferenceSystem.Terrain);
		}

		return new Geopoint(location.ToBasicGeoposition(), reference);
	}

	#endregion
}