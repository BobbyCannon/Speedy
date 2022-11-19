#region References

using System;
using System.Linq;
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

	/// <inheritdoc cref="ILocationDeviceInformation.HasValue" />
	public virtual bool HasValue
	{
		get => this.HasLocation();
		set => this.UpdateHasLocation(value);
	}

	/// <inheritdoc />
	public double Heading { get; set; }

	/// <summary>
	/// Represents a global unique ID to identify an information type.
	/// </summary>
	public abstract Guid InformationId { get; }

	/// <inheritdoc />
	public string ProviderName { get; set; }

	/// <inheritdoc />
	public string SourceName { get; set; }

	/// <inheritdoc />
	public double Speed { get; set; }

	/// <inheritdoc />
	public DateTime StatusTime { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Update the ILocationDeviceInformation with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public override bool UpdateWith(ILocationDeviceInformation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			Flags = update.Flags;
			// These values set by flags
			//HasHeading = update.HasHeading;
			//HasSpeed = update.HasSpeed;
			//HasValue = update.HasValue;
			Heading = update.Heading;
			Speed = update.Speed;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Flags)), x => x.Flags = update.Flags);
			// These values set by flags
			//this.IfThen(_ => !exclusions.Contains(nameof(HasHeading)), x => x.HasHeading = update.HasHeading);
			//this.IfThen(_ => !exclusions.Contains(nameof(HasSpeed)), x => x.HasSpeed = update.HasSpeed);
			//this.IfThen(_ => !exclusions.Contains(nameof(HasValue)), x => x.HasValue = update.HasValue);
			this.IfThen(_ => !exclusions.Contains(nameof(Heading)), x => x.Heading = update.Heading);
			this.IfThen(_ => !exclusions.Contains(nameof(Speed)), x => x.Speed = update.Speed);
		}

		return true;
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			ILocationDeviceInformation value => UpdateWith(value, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		this.HandleFlagsChanged(propertyName);
		base.OnPropertyChangedInDispatcher(propertyName);
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

	/// <inheritdoc cref="IDeviceInformation.HasValue" />
	new bool HasValue { get; set; }

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