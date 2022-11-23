#region References

using System;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Speedy.Data.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

/// <summary>
/// Extension for the Windows platform.
/// </summary>
public static class PlatformExtensions
{
	#region Methods

	/// <summary>
	/// Converts an IAsyncOperation to a ConfigurableTaskAwaitable.
	/// </summary>
	public static ConfiguredTaskAwaitable<T> AsTask<T>(this IAsyncOperation<T> self, bool continueOnCapturedContext)
	{
		return self.AsTask().ConfigureAwait(continueOnCapturedContext);
	}

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