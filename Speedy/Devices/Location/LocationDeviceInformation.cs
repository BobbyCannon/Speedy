#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents location information for a device.
/// </summary>
public abstract class LocationDeviceInformation
	: Bindable<ILocationDeviceInformation>,
		ILocationDeviceInformation
{
	#region Constructors

	/// <summary>
	/// Instantiates location information for a device.
	/// </summary>
	protected LocationDeviceInformation(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public double Accuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType AccuracyReference { get; set; }

	/// <inheritdoc />
	public LocationFlags Flags { get; set; }

	/// <inheritdoc />
	public bool HasAccuracy => this.HasAccuracy();

	/// <inheritdoc />
	public bool HasHeading
	{
		get => this.HasHeading();
		set => this.UpdateHasHeading(value);
	}

	/// <inheritdoc />
	public bool HasSpeed
	{
		get => this.HasSpeed();
		set => this.UpdateHasSpeed(value);
	}

	/// <inheritdoc />
	public virtual bool HasValue
	{
		get => this.HasLocation();
		set => this.UpdateHasLocation(value);
	}

	/// <inheritdoc />
	public double Heading { get; set; }

	/// <inheritdoc />
	public string SourceName { get; set; }

	/// <inheritdoc />
	public double Speed { get; set; }

	/// <inheritdoc />
	public DateTime StatusTime { get; set; }

	#endregion

	#region Methods

	public override bool UpdateWith(ILocationDeviceInformation update, params string[] exclusions)
	{
		return false;
	}

	#endregion
}

/// <summary>
/// Represents location information for a device.
/// </summary>
public interface ILocationDeviceInformation
	: IDeviceInformation,
		IUpdatable<ILocationDeviceInformation>
{
	#region Properties

	/// <summary>
	/// Flags for the location of the provider.
	/// </summary>
	LocationFlags Flags { get; set; }

	/// <summary>
	/// Specifies if the Heading value is valid.
	/// </summary>
	bool HasHeading { get; set; }

	/// <summary>
	/// Specifies if the Speed value is valid.
	/// </summary>
	bool HasSpeed { get; set; }

	/// <summary>
	/// The heading of a device.
	/// </summary>
	double Heading { get; set; }

	/// <summary>
	/// The speed of the device in meters per second.
	/// </summary>
	double Speed { get; set; }

	#endregion
}