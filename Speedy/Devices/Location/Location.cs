#region References

using System;
using System.Linq;
using Speedy.Extensions;
using Speedy.Protocols;
using Speedy.Storage;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a full location from a LocationProvider. Contains horizontal and vertical location.
/// </summary>
public class Location : BasicLocation, ILocation, ICloneable
{
	#region Constructors

	/// <summary>
	/// Instantiates a location for a LocationProvider.
	/// </summary>
	public Location() : this(null)
	{
	}

	/// <summary>
	/// Instantiates a location for a LocationProvider.
	/// </summary>
	public Location(IDispatcher dispatcher) : this(0, 0, 0, AltitudeReferenceType.Unspecified, dispatcher)
	{
	}

	/// <summary>
	/// Instantiates a location for a LocationProvider.
	/// </summary>
	public Location(IBasicLocation location, IDispatcher dispatcher = null)
		: this(location.Latitude, location.Longitude, location.Altitude, location.AltitudeReference, dispatcher)
	{
	}

	/// <summary>
	/// Instantiates a location for a LocationProvider.
	/// </summary>
	public Location(double latitude = 0, double longitude = 0, double altitude = 0, AltitudeReferenceType altitudeReference = AltitudeReferenceType.Unspecified, IDispatcher dispatcher = null)
		: base(latitude, longitude, altitude, altitudeReference, dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// Specifies if the Heading value is valid
	/// </summary>
	public bool HasHeading
	{
		get => this.HasHeading();
		set => this.UpdateHasHeading(value);
	}

	/// <summary>
	/// Specifies if the Accuracy value is valid
	/// </summary>
	public bool HasHorizontalAccuracy => this.HasSupportedHorizontalAccuracy();

	/// <summary>
	/// Specifies if the Speed value is valid
	/// </summary>
	public bool HasSpeed
	{
		get => this.HasSpeed();
		set => this.UpdateHasSpeed(value);
	}

	/// <summary>
	/// Specifies if the Altitude Accuracy value is valid
	/// </summary>
	public bool HasVerticalAccuracy => this.HasSupportedVerticalAccuracy();

	/// <inheritdoc />
	public double Heading { get; set; }

	/// <inheritdoc />
	public double HorizontalAccuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType HorizontalAccuracyReference { get; set; }

	/// <inheritdoc />
	public string HorizontalSourceName { get; set; }

	/// <inheritdoc />
	public DateTime HorizontalStatusTime { get; set; }

	/// <inheritdoc />
	public LocationFlags LocationFlags { get; set; }

	/// <inheritdoc />
	public string ProviderName { get; set; }

	/// <inheritdoc />
	public double Speed { get; set; }

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
	public object DeepClone(int? maxDepth = null)
	{
		return ShallowClone();
	}

	/// <inheritdoc />
	public object ShallowClone()
	{
		var response = new Location(Dispatcher);
		response.UpdateWith(this);
		return response;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Latitude:N7}°  {Longitude:N7}°  {Altitude:N2} {HorizontalAccuracyReference.ToDisplayShortName()}";
	}

	/// <summary>
	/// Update the Location with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public void UpdateWith(ILocation update, params string[] exclusions)
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
			HasHeading = update.HasHeading;
			HasSpeed = update.HasSpeed;
			Heading = update.Heading;
			HorizontalAccuracy = update.HorizontalAccuracy;
			HorizontalAccuracyReference = update.HorizontalAccuracyReference;
			HorizontalSourceName = update.HorizontalSourceName;
			HorizontalStatusTime = update.HorizontalStatusTime;
			Latitude = update.Latitude;
			LocationFlags = update.LocationFlags;
			Longitude = update.Longitude;
			ProviderName = update.ProviderName;
			Speed = update.Speed;
			VerticalAccuracy = update.VerticalAccuracy;
			VerticalAccuracyReference = update.VerticalAccuracyReference;
			VerticalSourceName = update.VerticalSourceName;
			VerticalStatusTime = update.VerticalStatusTime;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Altitude)), x => x.Altitude = update.Altitude);
			this.IfThen(_ => !exclusions.Contains(nameof(AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HasHeading)), x => x.HasHeading = update.HasHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HasSpeed)), x => x.HasSpeed = update.HasSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(Heading)), x => x.Heading = update.Heading);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalAccuracy)), x => x.HorizontalAccuracy = update.HorizontalAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalAccuracyReference)), x => x.HorizontalAccuracyReference = update.HorizontalAccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalSourceName)), x => x.HorizontalSourceName = update.HorizontalSourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalStatusTime)), x => x.HorizontalStatusTime = update.HorizontalStatusTime);
			this.IfThen(_ => !exclusions.Contains(nameof(Latitude)), x => x.Latitude = update.Latitude);
			this.IfThen(_ => !exclusions.Contains(nameof(LocationFlags)), x => x.LocationFlags = update.LocationFlags);
			this.IfThen(_ => !exclusions.Contains(nameof(Longitude)), x => x.Longitude = update.Longitude);
			this.IfThen(_ => !exclusions.Contains(nameof(ProviderName)), x => x.ProviderName = update.ProviderName);
			this.IfThen(_ => !exclusions.Contains(nameof(Speed)), x => x.Speed = update.Speed);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalAccuracy)), x => x.VerticalAccuracy = update.VerticalAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalAccuracyReference)), x => x.VerticalAccuracyReference = update.VerticalAccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalSourceName)), x => x.VerticalSourceName = update.VerticalSourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalStatusTime)), x => x.VerticalStatusTime = update.VerticalStatusTime);
		}

		//base.UpdateWith(update, exclusions);
	}

	/// <inheritdoc />
	public override void UpdateWith(object update, params string[] exclusions)
	{
		switch (update)
		{
			case Location options:
			{
				UpdateWith(options, exclusions);
				return;
			}
			case ILocation options:
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

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		this.ProcessOnPropertyChange(propertyName);
		this.CleanupLocation(propertyName);
		base.OnPropertyChangedInDispatcher(propertyName);
	}

	#endregion
}

/// <summary>
/// Represents a provider location.
/// </summary>
public interface ILocation : IBasicLocation, IUpdatable<ILocation>,
	IHorizontalLocation, IVerticalLocation, ILocationExtras
{
	#region Properties

	/// <summary>
	/// The name of the provider that is the source of this location.
	/// </summary>
	public string ProviderName { get; set; }

	#endregion
}

/// <summary>
/// Extra members for location.
/// </summary>
public interface ILocationExtras : IBindable
{
	#region Properties

	/// <summary>
	/// Specifies if the Heading value is valid
	/// </summary>
	bool HasHeading { get; set; }

	/// <summary>
	/// Specifies if the Speed value is valid
	/// </summary>
	bool HasSpeed { get; set; }

	/// <summary>
	/// The heading of a device.
	/// </summary>
	double Heading { get; set; }

	/// <summary>
	/// Flags for the location of the provider.
	/// </summary>
	LocationFlags LocationFlags { get; set; }

	/// <summary>
	/// The speed of the device in meters per second.
	/// </summary>
	double Speed { get; set; }

	#endregion
}