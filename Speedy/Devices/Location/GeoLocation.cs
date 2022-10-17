#region References

using System;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy.Devices.Location
{
	/// <summary>
	/// Represents a geo-location (lat, long, alt, alt ref).
	/// </summary>
	public class GeoLocation : Bindable, IGeoLocation, IComparable, IComparable<GeoLocation>, IEquatable<GeoLocation>, IUpdatable<IGeoLocation>
	{
		#region Constructors

		/// <summary>
		/// This constructor is only for serialization, do not actually use.
		/// </summary>
		public GeoLocation() : this(null)
		{
		}

		/// <summary>
		/// Initialize an instance of the GeoLocation.
		/// </summary>
		/// <param name="location"> The default values. </param>
		public GeoLocation(IGeoLocation location)
			: this(location.Latitude, location.Longitude, location.Altitude, location.AltitudeReference)
		{
		}

		/// <summary>
		/// Initialize an instance of the GeoLocation.
		/// </summary>
		/// <param name="latitude"> The default values. </param>
		/// <param name="longitude"> The default values. </param>
		public GeoLocation(double latitude, double longitude) : this(null, latitude, longitude)
		{
		}

		/// <summary>
		/// Initialize an instance of the GeoLocation.
		/// </summary>
		/// <param name="latitude"> The default value. </param>
		/// <param name="longitude"> The default value. </param>
		/// <param name="altitude"> The default value. </param>
		/// <param name="altitudeReference"> The default value. </param>
		public GeoLocation(double latitude, double longitude, double altitude, AltitudeReferenceType altitudeReference) : this(null, latitude, longitude, altitude, altitudeReference)
		{
		}

		/// <summary>
		/// Initialize an instance of the GeoLocation.
		/// </summary>
		/// <param name="dispatcher"> The default value. </param>
		/// <param name="latitude"> The default value. </param>
		/// <param name="longitude"> The default value. </param>
		/// <param name="altitude"> The default value. </param>
		/// <param name="altitudeReference"> The default value. </param>
		public GeoLocation(IDispatcher dispatcher, double latitude = 0, double longitude = 0, double altitude = 0, AltitudeReferenceType altitudeReference = AltitudeReferenceType.Unspecified) : base(dispatcher)
		{
			Latitude = latitude;
			Longitude = longitude;
			Altitude = altitude;
			AltitudeReference = altitudeReference;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public double Altitude { get; set; }

		/// <inheritdoc />
		public AltitudeReferenceType AltitudeReference { get; set; }

		/// <inheritdoc />
		public double Latitude { get; set; }

		/// <inheritdoc />
		public double Longitude { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public int CompareTo(GeoLocation other)
		{
			var altitude = Altitude.CompareTo(other.Altitude);
			var altitudeReference = AltitudeReference.CompareTo(other.AltitudeReference);
			var latitude = Latitude.CompareTo(other.Latitude);
			var longitude = Longitude.CompareTo(other.Longitude);

			return (altitude == 0) && (altitudeReference == 0) && (latitude == 0) && (longitude == 0) ? 0 : 1;
		}

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			return CompareTo(obj as GeoLocation);
		}

		/// <inheritdoc />
		public bool Equals(GeoLocation other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Altitude.Equals(other.Altitude)
				&& AltitudeReference.Equals(other.AltitudeReference)
				&& Latitude.Equals(other.Latitude)
				&& Longitude.Equals(other.Longitude);
		}

		/// <inheritdoc />
		public void UpdateWith(IGeoLocation update, params string[] exclusions)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return Equals(obj as GeoLocation);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Altitude.GetHashCode();
				hashCode = (hashCode * 397) ^ AltitudeReference.GetHashCode();
				hashCode = (hashCode * 397) ^ Latitude.GetHashCode();
				hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Latitude:F7}, {Longitude:F7}, {Altitude:F3} / {AltitudeReference.GetDisplayName()}";
		}

		#endregion
	}

	/// <summary>
	/// Represents a geo-location (lat, long, alt, alt ref).
	/// </summary>
	public interface IGeoLocation : IHorizontalLocation, IVerticalLocation
	{
	}
}