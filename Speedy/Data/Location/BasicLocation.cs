#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Data.Location;

/// <summary>
/// Represents a minimal location (lat, long, alt, alt ref).
/// </summary>
public class BasicLocation
	: Bindable<BasicLocation>,
		IUpdatable<ILocation<IHorizontalLocation, IVerticalLocation>>,
		IBasicLocation, IComparable, IComparable<BasicLocation>,
		IEquatable<BasicLocation>
{
	#region Constructors

	/// <summary>
	/// This constructor is only for serialization, do not actually use.
	/// </summary>
	public BasicLocation() : this(null)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	public BasicLocation(IDispatcher dispatcher) : this(0, 0, 0, AltitudeReferenceType.Unspecified, dispatcher)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	public BasicLocation(IBasicLocation location, IDispatcher dispatcher = null)
		: this(location.Latitude, location.Longitude, location.Altitude, location.AltitudeReference, dispatcher)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	/// <param name="latitude"> The default value. </param>
	/// <param name="longitude"> The default value. </param>
	/// <param name="altitude"> The default value. </param>
	/// <param name="altitudeReference"> The default value. </param>
	/// <param name="dispatcher"> The default value. </param>
	public BasicLocation(double latitude = 0, double longitude = 0, double altitude = 0, AltitudeReferenceType altitudeReference = AltitudeReferenceType.Unspecified, IDispatcher dispatcher = null) : base(dispatcher)
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

	/// <summary>
	/// Check a location to determine if <see cref="IMinimalVerticalLocation.Altitude" /> is available.
	/// </summary>
	public bool HasAltitude => this.HasSupportedAltitude();

	/// <inheritdoc />
	public double Latitude { get; set; }

	/// <inheritdoc />
	public double Longitude { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public int CompareTo(BasicLocation other)
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
		return CompareTo(obj as BasicLocation);
	}

	/// <inheritdoc />
	public bool Equals(BasicLocation other)
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
	public override bool Equals(object obj)
	{
		return Equals(obj as BasicLocation);
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

	/// <summary>
	/// Get a IBasicLocation from a Location.
	/// </summary>
	/// <param name="location"> The location. </param>
	/// <returns> The equivalent value as a basic location. </returns>
	public static implicit operator BasicLocation(Location location)
	{
		var response = new BasicLocation(location.GetDispatcher());
		var horizontalLocation = location.HorizontalLocation;
		var verticalLocation = location.VerticalLocation;

		if (horizontalLocation != null)
		{
			response.Latitude = horizontalLocation.Latitude;
			response.Longitude = horizontalLocation.Longitude;
		}

		if (verticalLocation != null)
		{
			response.AltitudeReference = verticalLocation.AltitudeReference;
			response.Altitude = verticalLocation.Altitude;
		}

		return response;
	}

	/// <inheritdoc />
	public bool Refresh(ILocation<IHorizontalLocation, IVerticalLocation> update, params string[] exclusions)
	{
		return this.Refresh<ILocation<IHorizontalLocation, IVerticalLocation>>(update, exclusions);
	}

	/// <inheritdoc />
	public bool ShouldUpdate(ILocation<IHorizontalLocation, IVerticalLocation> update)
	{
		return ShouldUpdate(update.HorizontalLocation)
			|| ShouldUpdate(update.VerticalLocation);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Latitude:F7}, {Longitude:F7}, {Altitude:F3} / {AltitudeReference.GetDisplayName()}";
	}

	/// <inheritdoc />
	public override bool UpdateWith(BasicLocation update, params string[] exclusions)
	{
		return UpdateWith(update, exclusions);
	}

	/// <summary>
	/// Update the BasicLocation with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public bool UpdateWith(IBasicLocation update, params string[] exclusions)
	{
		return LocationExtensions.UpdateWith(this, update, exclusions);
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			BasicLocation options => UpdateWith(options, exclusions),
			IBasicLocation options => UpdateWith(options, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	/// <inheritdoc />
	public bool UpdateWith(ILocation<IHorizontalLocation, IVerticalLocation> update, params string[] exclusions)
	{
		return LocationExtensions.UpdateWith(this, update, exclusions);
	}

	#endregion
}

/// <summary>
/// Represents a minimal location (lat, long, alt, alt ref).
/// </summary>
public interface IBasicLocation : IMinimalHorizontalLocation, IMinimalVerticalLocation
{
}