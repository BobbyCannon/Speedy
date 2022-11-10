#region References

using System;
using System.Linq;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a minimal location (lat, long, alt, alt ref).
/// </summary>
public class BasicLocation : Bindable, IBasicLocation,
	IComparable, IComparable<BasicLocation>,
	IEquatable<BasicLocation>, IUpdatable<IBasicLocation>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Latitude:F7}, {Longitude:F7}, {Altitude:F3} / {AltitudeReference.GetDisplayName()}";
	}

	/// <summary>
	/// Update the BasicLocation with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public void UpdateWith(IBasicLocation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			Altitude = update.Altitude;
			AltitudeReference = update.AltitudeReference;
			Latitude = update.Latitude;
			Longitude = update.Longitude;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Altitude)), x => x.Altitude = update.Altitude);
			this.IfThen(_ => !exclusions.Contains(nameof(AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
			this.IfThen(_ => !exclusions.Contains(nameof(Latitude)), x => x.Latitude = update.Latitude);
			this.IfThen(_ => !exclusions.Contains(nameof(Longitude)), x => x.Longitude = update.Longitude);
		}

		//base.UpdateWith(update, exclusions);
	}

	/// <inheritdoc />
	public override void UpdateWith(object update, params string[] exclusions)
	{
		switch (update)
		{
			case BasicLocation options:
			{
				UpdateWith(options, exclusions);
				return;
			}
			case IBasicLocation options:
			{
				UpdateWith(options, exclusions);
				return;
			}
			default:
			{
				base.UpdateWith(update, exclusions);
				return;
			}
		}
	}

	#endregion
}

/// <summary>
/// Represents a minimal location (lat, long, alt, alt ref).
/// </summary>
public interface IBasicLocation : IMinimalHorizontalLocation, IMinimalVerticalLocation
{
}