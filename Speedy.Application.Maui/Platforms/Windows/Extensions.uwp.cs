#region References

using System.Runtime.CompilerServices;
using Windows.Foundation;
using AltitudeReferenceSystem = Windows.Devices.Geolocation.AltitudeReferenceSystem;
using Speedy.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Maui
{
	public static class ExtensionsForUwp
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
				AltitudeReferenceType.Surface => AltitudeReferenceSystem.Surface,
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
				AltitudeReferenceSystem.Surface => AltitudeReferenceType.Surface,
				AltitudeReferenceSystem.Unspecified => AltitudeReferenceType.Unspecified,
				_ => AltitudeReferenceType.Unspecified
			};
		}

		#endregion
	}
}