#region References

using Speedy.Data.Location;
using AltitudeReferenceSystem = Windows.Devices.Geolocation.AltitudeReferenceSystem;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Maui;

/// <summary>
/// Extension for the Windows platform.
/// </summary>
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

	#endregion
}