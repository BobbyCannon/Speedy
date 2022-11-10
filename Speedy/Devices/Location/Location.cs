#region References

using System;
using System.Linq;
using Speedy.Extensions;
using Speedy.Protocols;
using Speedy.Serialization;
using Speedy.Storage;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a full location from a LocationProvider. Contains horizontal and vertical location.
/// </summary>
public class Location : CloneableBindable<Location, ILocation>, ILocation
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
		: base(dispatcher)
	{
		Altitude = altitude;
		AltitudeReference = altitudeReference;
		Latitude = latitude;
		Longitude = longitude;
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
	public bool HasHorizontalAccuracy => this.HasSupportedHorizontalAccuracy();

	/// <inheritdoc />
	public bool HasHorizontalHeading
	{
		get => this.HasHorizontalHeading();
		set => this.UpdateHorizontalHeading(value);
	}

	/// <inheritdoc />
	public bool HasHorizontalSpeed
	{
		get => this.HasHorizontalSpeed();
		set => this.UpdateHorizontalSpeed(value);
	}

	/// <inheritdoc />
	public bool HasVerticalAccuracy => this.HasSupportedVerticalAccuracy();

	/// <inheritdoc />
	public bool HasVerticalHeading
	{
		get => this.HasVerticalSpeed();
		set => this.UpdateVerticalSpeed(value);
	}

	/// <inheritdoc />
	public bool HasVerticalSpeed
	{
		get => this.HasVerticalSpeed();
		set => this.UpdateVerticalSpeed(value);
	}

	/// <inheritdoc />
	public double HorizontalAccuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType HorizontalAccuracyReference { get; set; }

	/// <inheritdoc />
	public LocationFlags HorizontalFlags { get; set; }

	/// <inheritdoc />
	public double HorizontalHeading { get; set; }

	/// <inheritdoc />
	public string HorizontalSourceName { get; set; }

	/// <inheritdoc />
	public double HorizontalSpeed { get; set; }

	/// <inheritdoc />
	public DateTime HorizontalStatusTime { get; set; }

	/// <inheritdoc />
	public double Latitude { get; set; }

	/// <inheritdoc />
	public double Longitude { get; set; }

	/// <inheritdoc />
	public string ProviderName { get; set; }

	/// <inheritdoc />
	public double VerticalAccuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType VerticalAccuracyReference { get; set; }

	/// <inheritdoc />
	public LocationFlags VerticalFlags { get; set; }

	/// <inheritdoc />
	public double VerticalHeading { get; set; }

	/// <inheritdoc />
	public string VerticalSourceName { get; set; }

	/// <inheritdoc />
	public double VerticalSpeed { get; set; }

	/// <inheritdoc />
	public DateTime VerticalStatusTime { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Latitude:N7}°  {Longitude:N7}°  {Altitude:N2} {VerticalAccuracyReference.ToDisplayShortName()}";
	}

	/// <inheritdoc />
	public void UpdateWith(IVerticalLocation update, params string[] exclusions)
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
			HasVerticalHeading = update.HasVerticalHeading;
			HasVerticalSpeed = update.HasVerticalSpeed;
			VerticalAccuracy = update.VerticalAccuracy;
			VerticalAccuracyReference = update.VerticalAccuracyReference;
			VerticalFlags = update.VerticalFlags;
			VerticalHeading = update.VerticalHeading;
			VerticalSourceName = update.VerticalSourceName;
			VerticalSpeed = update.VerticalSpeed;
			VerticalStatusTime = update.VerticalStatusTime;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Altitude)), x => x.Altitude = update.Altitude);
			this.IfThen(_ => !exclusions.Contains(nameof(AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HasVerticalHeading)), x => x.HasVerticalHeading = update.HasVerticalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HasVerticalSpeed)), x => x.HasVerticalSpeed = update.HasVerticalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalAccuracy)), x => x.VerticalAccuracy = update.VerticalAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalAccuracyReference)), x => x.VerticalAccuracyReference = update.VerticalAccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalFlags)), x => x.VerticalFlags = update.VerticalFlags);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalHeading)), x => x.VerticalHeading = update.VerticalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalSourceName)), x => x.VerticalSourceName = update.VerticalSourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalSpeed)), x => x.VerticalSpeed = update.VerticalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalStatusTime)), x => x.VerticalStatusTime = update.VerticalStatusTime);
		}
	}

	/// <inheritdoc />
	public void UpdateWith(IHorizontalLocation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			HasHorizontalHeading = update.HasHorizontalHeading;
			HasHorizontalSpeed = update.HasHorizontalSpeed;
			HorizontalAccuracy = update.HorizontalAccuracy;
			HorizontalAccuracyReference = update.HorizontalAccuracyReference;
			HorizontalFlags = update.HorizontalFlags;
			HorizontalHeading = update.HorizontalHeading;
			HorizontalSourceName = update.HorizontalSourceName;
			HorizontalSpeed = update.HorizontalSpeed;
			HorizontalStatusTime = update.HorizontalStatusTime;
			Latitude = update.Latitude;
			Longitude = update.Longitude;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(HasHorizontalHeading)), x => x.HasHorizontalHeading = update.HasHorizontalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HasHorizontalSpeed)), x => x.HasHorizontalSpeed = update.HasHorizontalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalAccuracy)), x => x.HorizontalAccuracy = update.HorizontalAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalAccuracyReference)), x => x.HorizontalAccuracyReference = update.HorizontalAccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalFlags)), x => x.HorizontalFlags = update.HorizontalFlags);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalHeading)), x => x.HorizontalHeading = update.HorizontalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalSourceName)), x => x.HorizontalSourceName = update.HorizontalSourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalSpeed)), x => x.HorizontalSpeed = update.HorizontalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalStatusTime)), x => x.HorizontalStatusTime = update.HorizontalStatusTime);
			this.IfThen(_ => !exclusions.Contains(nameof(Latitude)), x => x.Latitude = update.Latitude);
			this.IfThen(_ => !exclusions.Contains(nameof(Longitude)), x => x.Longitude = update.Longitude);
		}
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
			HasHorizontalHeading = update.HasHorizontalHeading;
			HasHorizontalSpeed = update.HasHorizontalSpeed;
			HasVerticalHeading = update.HasVerticalHeading;
			HasVerticalSpeed = update.HasVerticalSpeed;
			HorizontalAccuracy = update.HorizontalAccuracy;
			HorizontalAccuracyReference = update.HorizontalAccuracyReference;
			HorizontalFlags = update.HorizontalFlags;
			HorizontalHeading = update.HorizontalHeading;
			HorizontalSourceName = update.HorizontalSourceName;
			HorizontalSpeed = update.HorizontalSpeed;
			HorizontalStatusTime = update.HorizontalStatusTime;
			Latitude = update.Latitude;
			Longitude = update.Longitude;
			ProviderName = update.ProviderName;
			VerticalAccuracy = update.VerticalAccuracy;
			VerticalAccuracyReference = update.VerticalAccuracyReference;
			VerticalFlags = update.VerticalFlags;
			VerticalHeading = update.VerticalHeading;
			VerticalSourceName = update.VerticalSourceName;
			VerticalSpeed = update.VerticalSpeed;
			VerticalStatusTime = update.VerticalStatusTime;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Altitude)), x => x.Altitude = update.Altitude);
			this.IfThen(_ => !exclusions.Contains(nameof(AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HasHorizontalHeading)), x => x.HasHorizontalHeading = update.HasHorizontalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HasHorizontalSpeed)), x => x.HasHorizontalSpeed = update.HasHorizontalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(HasVerticalHeading)), x => x.HasVerticalHeading = update.HasVerticalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HasVerticalSpeed)), x => x.HasVerticalSpeed = update.HasVerticalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalAccuracy)), x => x.HorizontalAccuracy = update.HorizontalAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalAccuracyReference)), x => x.HorizontalAccuracyReference = update.HorizontalAccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalFlags)), x => x.HorizontalFlags = update.HorizontalFlags);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalHeading)), x => x.HorizontalHeading = update.HorizontalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalSourceName)), x => x.HorizontalSourceName = update.HorizontalSourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalSpeed)), x => x.HorizontalSpeed = update.HorizontalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalStatusTime)), x => x.HorizontalStatusTime = update.HorizontalStatusTime);
			this.IfThen(_ => !exclusions.Contains(nameof(Latitude)), x => x.Latitude = update.Latitude);
			this.IfThen(_ => !exclusions.Contains(nameof(Longitude)), x => x.Longitude = update.Longitude);
			this.IfThen(_ => !exclusions.Contains(nameof(ProviderName)), x => x.ProviderName = update.ProviderName);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalAccuracy)), x => x.VerticalAccuracy = update.VerticalAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalAccuracyReference)), x => x.VerticalAccuracyReference = update.VerticalAccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalFlags)), x => x.VerticalFlags = update.VerticalFlags);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalHeading)), x => x.VerticalHeading = update.VerticalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalSourceName)), x => x.VerticalSourceName = update.VerticalSourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalSpeed)), x => x.VerticalSpeed = update.VerticalSpeed);
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
	public override void UpdateWith(Location update, params string[] exclusions)
	{
		UpdateWith(update, exclusions);
	}

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		this.ProcessOnPropertyChange(propertyName);
		this.CleanupLocation(propertyName);
		base.OnPropertyChangedInDispatcher(propertyName);
	}

	/// <inheritdoc />
	IHorizontalLocation ICloneable<IHorizontalLocation>.DeepClone(int? maxDepth)
	{
		var response = new HorizontalLocation(Dispatcher);
		response.UpdateWith(this);
		return response;
	}

	/// <inheritdoc />
	IVerticalLocation ICloneable<IVerticalLocation>.DeepClone(int? maxDepth)
	{
		var response = new VerticalLocation(Dispatcher);
		response.UpdateWith(this);
		return response;
	}

	/// <inheritdoc />
	IHorizontalLocation ICloneable<IHorizontalLocation>.ShallowClone()
	{
		var response = new HorizontalLocation(Dispatcher);
		response.UpdateWith(this);
		return response;
	}

	/// <inheritdoc />
	IVerticalLocation ICloneable<IVerticalLocation>.ShallowClone()
	{
		var response = new VerticalLocation(Dispatcher);
		response.UpdateWith(this);
		return response;
	}

	#endregion
}

/// <summary>
/// Represents a provider location.
/// </summary>
public interface ILocation : IBasicLocation,
	ICloneable<ILocation>, IUpdatable<ILocation>,
	IHorizontalLocation, IVerticalLocation
{
	#region Properties

	/// <summary>
	/// The name of the provider that is the source of this location.
	/// </summary>
	public string ProviderName { get; set; }

	#endregion
}