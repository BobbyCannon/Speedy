#region References

using System;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a minimal location (lat, long, alt, alt ref).
/// </summary>
public class VerticalLocation : Bindable, IMinimalVerticalLocation, IComparable, IComparable<VerticalLocation>, IEquatable<VerticalLocation>, IUpdatable<IMinimalVerticalLocation>
{
	#region Constructors

	/// <summary>
	/// This constructor is only for serialization, do not actually use.
	/// </summary>
	public VerticalLocation() : this(null)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	public VerticalLocation(IDispatcher dispatcher) : this(0, AltitudeReferenceType.Unspecified, dispatcher)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	public VerticalLocation(IMinimalVerticalLocation location, IDispatcher dispatcher = null)
		: this(location.Altitude, location.AltitudeReference, dispatcher)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	/// <param name="altitude"> The default value. </param>
	/// <param name="altitudeReference"> The default value. </param>
	/// <param name="dispatcher"> The default value. </param>
	public VerticalLocation(double altitude = 0, AltitudeReferenceType altitudeReference = AltitudeReferenceType.Unspecified, IDispatcher dispatcher = null) : base(dispatcher)
	{
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

	#endregion

	#region Methods

	/// <inheritdoc />
	public int CompareTo(VerticalLocation other)
	{
		var altitude = Altitude.CompareTo(other.Altitude);
		var altitudeReference = AltitudeReference.CompareTo(other.AltitudeReference);

		return (altitude == 0) && (altitudeReference == 0) ? 0 : 1;
	}

	/// <inheritdoc />
	public int CompareTo(object obj)
	{
		return CompareTo(obj as VerticalLocation);
	}

	/// <inheritdoc />
	public bool Equals(VerticalLocation other)
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
			&& AltitudeReference.Equals(other.AltitudeReference);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return Equals(obj as VerticalLocation);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = Altitude.GetHashCode();
			hashCode = (hashCode * 397) ^ AltitudeReference.GetHashCode();
			return hashCode;
		}
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Altitude:F3} / {AltitudeReference.GetDisplayName()}";
	}

	/// <inheritdoc />
	public void UpdateWith(IMinimalVerticalLocation update, params string[] exclusions)
	{
		this.UpdateWithUsingReflection(update, exclusions);
	}

	#endregion
}