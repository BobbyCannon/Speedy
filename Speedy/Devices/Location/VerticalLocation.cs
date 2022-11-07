#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a vertical location.
/// </summary>
public class VerticalLocation : Bindable, IVerticalLocation
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

	/// <inheritdoc />
	public bool HasVerticalAccuracy => this.HasSupportedVerticalAccuracy();

	/// <inheritdoc />
	public double VerticalAccuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType VerticalAccuracyReference { get; set; }

	/// <inheritdoc />
	public string VerticalSourceName { get; set; }

	/// <inheritdoc />
	public DateTime VerticalStatusTime { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Altitude:F3} / {AltitudeReference.GetDisplayName()}";
	}

	#endregion
}