#region References

using System;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Speedy.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

public static class PlatformExtensions
{
	#region Methods

	public static ConfiguredTaskAwaitable<T> AsTask<T>(this IAsyncOperation<T> self, bool continueOnCapturedContext)
	{
		return self.AsTask().ConfigureAwait(continueOnCapturedContext);
	}

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