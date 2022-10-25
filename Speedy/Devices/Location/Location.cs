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
/// Represents a full location from a LocationProvider.
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

	/// <inheritdoc />
	public double Accuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType AccuracyReference { get; set; }

	/// <inheritdoc />
	public double AltitudeAccuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType AltitudeAccuracyReference { get; set; }

	/// <summary>
	/// Specifies if the Accuracy value is valid
	/// </summary>
	public bool HasAccuracy => this.HasSupportedAccuracy();

	/// <summary>
	/// Specifies if the Altitude value is valid
	/// </summary>
	public bool HasAltitude => this.HasSupportedAltitude();

	/// <summary>
	/// Specifies if the Altitude Accuracy value is valid
	/// </summary>
	public bool HasAltitudeAccuracy => this.HasSupportedAltitudeAccuracy();

	/// <summary>
	/// Specifies if the Heading value is valid
	/// </summary>
	public bool HasHeading
	{
		get => this.HasHeading();
		set => this.UpdateHasHeading(value);
	}

	/// <summary>
	/// Specifies if the Speed value is valid
	/// </summary>
	public bool HasSpeed
	{
		get => this.HasSpeed();
		set => this.UpdateHasSpeed(value);
	}

	/// <inheritdoc />
	public double Heading { get; set; }

	/// <inheritdoc />
	public LocationFlags LocationFlags { get; set; }

	/// <inheritdoc />
	public string SourceName { get; set; }

	/// <inheritdoc />
	public double Speed { get; set; }

	/// <inheritdoc />
	public DateTime StatusTime { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public object DeepClone(int? maxDepth = null)
	{
		return ShallowClone();
	}

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		this.ProcessOnPropertyChange(propertyName);
		this.CleanupLocation(propertyName);
		base.OnPropertyChangedInDispatcher(propertyName);
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
		return $"{Latitude:N7}°  {Longitude:N7}°  {Altitude:N2} {AccuracyReference.ToDisplayShortName()}";
	}

	/// <summary>
	/// Update the ProviderLocation with an update.
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
			Accuracy = update.Accuracy;
			Altitude = update.Altitude;
			AltitudeAccuracy = update.AltitudeAccuracy;
			AltitudeReference = update.AltitudeReference;
			HasHeading = update.HasHeading;
			HasSpeed = update.HasSpeed;
			Heading = update.Heading;
			Latitude = update.Latitude;
			Longitude = update.Longitude;
			SourceName = update.SourceName;
			Speed = update.Speed;
			StatusTime = update.StatusTime;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Accuracy)), x => x.Accuracy = update.Accuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(Altitude)), x => x.Altitude = update.Altitude);
			this.IfThen(_ => !exclusions.Contains(nameof(AltitudeAccuracy)), x => x.AltitudeAccuracy = update.AltitudeAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HasHeading)), x => x.HasHeading = update.HasHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HasSpeed)), x => x.HasSpeed = update.HasSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(Heading)), x => x.Heading = update.Heading);
			this.IfThen(_ => !exclusions.Contains(nameof(Latitude)), x => x.Latitude = update.Latitude);
			this.IfThen(_ => !exclusions.Contains(nameof(Longitude)), x => x.Longitude = update.Longitude);
			this.IfThen(_ => !exclusions.Contains(nameof(SourceName)), x => x.SourceName = update.SourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(Speed)), x => x.Speed = update.Speed);
			this.IfThen(_ => !exclusions.Contains(nameof(StatusTime)), x => x.StatusTime = update.StatusTime);
		}

		base.UpdateWith(update, exclusions);
	}

	/// <inheritdoc />
	public override void UpdateWith(object update, params string[] exclusions)
	{
		switch (update)
		{
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

	#endregion
}

/// <summary>
/// Represents a provider location.
/// </summary>
public interface ILocation : IBasicLocation, IUpdatable<ILocation>
{
	#region Properties

	/// <summary>
	/// The accuracy of the horizontal location. (latitude, longitude)
	/// </summary>
	double Accuracy { get; set; }

	/// <summary>
	/// The reference system for accuracy.
	/// </summary>
	AccuracyReferenceType AccuracyReference { get; set; }

	/// <summary>
	/// The accuracy of the vertical location. (altitude)
	/// </summary>
	double AltitudeAccuracy { get; set; }

	/// <summary>
	/// The reference system for altitude accuracy.
	/// </summary>
	AccuracyReferenceType AltitudeAccuracyReference { get; set; }

	/// <summary>
	/// Specifies if the Accuracy value is valid.
	/// </summary>
	bool HasAccuracy { get; }

	/// <summary>
	/// Specifies if the Altitude value is valid
	/// </summary>
	bool HasAltitude { get; }
	
	/// <summary>
	/// Specifies if the Altitude Accuracy value is valid
	/// </summary>
	bool HasAltitudeAccuracy { get; }

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
	/// The name of the source of the location. Ex. Wifi, GPS, Hardware, Simulated, etc
	/// </summary>
	string SourceName { get; set; }

	/// <summary>
	/// The speed of the device in meters per second.
	/// </summary>
	double Speed { get; set; }

	/// <summary>
	/// The original time of the location was captured.
	/// </summary>
	DateTime StatusTime { get; set; }

	#endregion
}