#region References

using Speedy.Devices.Location;
using static System.Math;

#endregion

namespace Speedy.Plugins.Devices.Location
{
    /// <summary>
    /// Geolocator Plugin Utilities
    /// </summary>
    public static class GeolocatorUtils
	{
		#region Methods

		/// <summary>
		/// Calculates the distance in miles
		/// </summary>
		/// <returns> The distance. </returns>
		/// <param name="latitudeStart"> Latitude start. </param>
		/// <param name="longitudeStart"> Longitude start. </param>
		/// <param name="latitudeEnd"> Latitude end. </param>
		/// <param name="longitudeEnd"> Longitude end. </param>
		/// <param name="units"> Units to return </param>
		public static double CalculateDistance(double latitudeStart, double longitudeStart,
			double latitudeEnd, double longitudeEnd, DistanceUnits units = DistanceUnits.Miles)
		{
			if ((latitudeEnd == latitudeStart) && (longitudeEnd == longitudeStart))
			{
				return 0;
			}

			var rlat1 = (PI * latitudeStart) / 180.0;
			var rlat2 = (PI * latitudeEnd) / 180.0;
			var theta = longitudeStart - longitudeEnd;
			var rtheta = (PI * theta) / 180.0;
			var dist = (Sin(rlat1) * Sin(rlat2)) + (Cos(rlat1) * Cos(rlat2) * Cos(rtheta));
			dist = Acos(dist);
			dist = (dist * 180.0) / PI;
			var final = dist * 60.0 * 1.1515;
			if (double.IsNaN(final) || double.IsInfinity(final) || double.IsNegativeInfinity(final) ||
				double.IsPositiveInfinity(final) || (final < 0))
			{
				return 0;
			}

			if (units == DistanceUnits.Kilometers)
			{
				return MilesToKilometers(final);
			}

			return final;
		}

		/// <summary>
		/// Calculates the distance in miles
		/// </summary>
		/// <returns> The distance. </returns>
		/// <param name="positionStart"> Start position </param>
		/// <param name="positionEnd"> End ProviderLocation. </param>
		/// <param name="units"> Units to return </param>
		public static double CalculateDistance(this IGeoLocation positionStart, IGeoLocation positionEnd, DistanceUnits units = DistanceUnits.Miles)
		{
			return CalculateDistance(positionStart.Latitude, positionStart.Longitude, positionEnd.Latitude, positionEnd.Longitude, units);
		}

		/// <summary>
		/// Convert Kilometers to Miles
		/// </summary>
		/// <param name="kilometers"> </param>
		/// <returns> </returns>
		public static double KilometersToMiles(double kilometers)
		{
			return kilometers * .62137119;
		}

		/// <summary>
		/// Convert Miles to Kilometers
		/// </summary>
		/// <param name="miles"> </param>
		/// <returns> </returns>
		public static double MilesToKilometers(double miles)
		{
			return miles * 1.609344;
		}

		#endregion

		#region Enumerations

		/// <summary>
		/// Units for the distance
		/// </summary>
		public enum DistanceUnits
		{
			/// <summary>
			/// Kilometers
			/// </summary>
			Kilometers,

			/// <summary>
			/// Miles
			/// </summary>
			Miles
		}

		#endregion
	}
}