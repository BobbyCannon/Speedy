#region References

using CoreLocation;
using Foundation;
using Speedy.Plugins.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui
{
	public static class GeolocationUtils
	{
		#region Methods

		public static DateTime ToDateTime(this NSDate date)
		{
			return (DateTime) date;
		}

		internal static IEnumerable<Address> ToAddresses(this IEnumerable<CLPlacemark> addresses)
		{
			return addresses.Select(address => new Address
			{
				Longitude = address.Location.Coordinate.Longitude,
				Latitude = address.Location.Coordinate.Latitude,
				FeatureName = address.Name,
				PostalCode = address.PostalCode,
				SubLocality = address.SubLocality,
				CountryCode = address.IsoCountryCode,
				CountryName = address.Country,
				Thoroughfare = address.Thoroughfare,
				SubThoroughfare = address.SubThoroughfare,
				Locality = address.Locality,
				AdminArea = address.AdministrativeArea,
				SubAdminArea = address.SubAdministrativeArea
			});
		}

		#endregion
	}
}