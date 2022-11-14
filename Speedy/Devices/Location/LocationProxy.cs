#region References

using System;
using Speedy.Storage;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a location proxy that generalizes accessors.
/// </summary>
public class LocationProxy : Bindable, ILocationProxy
{
	#region Fields

	private readonly IHorizontalLocation _hLocation;
	private readonly IVerticalLocation _vLocation;

	#endregion

	#region Constructors

	/// <summary>
	/// Create a proxy for a horizontal location.
	/// </summary>
	public LocationProxy(IHorizontalLocation location)
	{
		_hLocation = location;

		ProxyType = LocationProxyType.Horizontal;
	}

	/// <summary>
	/// Create a proxy for a vertical location.
	/// </summary>
	public LocationProxy(IVerticalLocation location)
	{
		_vLocation = location;

		ProxyType = LocationProxyType.Vertical;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The accuracy of the horizontal location. (latitude, longitude)
	/// </summary>
	public double Accuracy => _hLocation?.HorizontalAccuracy ?? _vLocation?.VerticalAccuracy ?? 0;

	/// <summary>
	/// The reference system for accuracy.
	/// </summary>
	public AccuracyReferenceType AccuracyReference => _hLocation?.HorizontalAccuracyReference ?? _vLocation?.VerticalAccuracyReference ?? AccuracyReferenceType.Unspecified;

	/// <summary>
	/// Flags for the location of the provider.
	/// </summary>
	public LocationFlags Flags => _hLocation?.HorizontalFlags ?? _vLocation?.VerticalFlags ?? LocationFlags.None;

	/// <summary>
	/// Specifies if the Accuracy value is valid.
	/// </summary>
	public bool HasAccuracy => _hLocation?.HasHorizontalAccuracy ?? _vLocation?.HasVerticalAccuracy ?? false;

	/// <summary>
	/// Specifies if the Heading value is valid
	/// </summary>
	public bool HasHeading => _hLocation?.HasHorizontalHeading ?? _vLocation?.HasVerticalHeading ?? false;

	/// <summary>
	/// The location has a location.
	/// </summary>
	public bool HasLocation => _vLocation?.HasAltitude ?? true;

	/// <summary>
	/// Specifies if the Speed value is valid
	/// </summary>
	public bool HasSpeed => _hLocation?.HasHorizontalSpeed ?? _vLocation?.HasVerticalSpeed ?? false;

	/// <summary>
	/// The heading of a device.
	/// </summary>
	public double Heading => _hLocation?.HorizontalHeading ?? _vLocation?.VerticalHeading ?? 0;

	/// <inheritdoc />
	public LocationProxyType ProxyType { get; }

	/// <summary>
	/// The name of the source of the location. Ex. Wifi, GPS, Hardware, Simulated, etc
	/// </summary>
	public string SourceName => _hLocation?.HorizontalSourceName ?? _vLocation?.VerticalSourceName ?? string.Empty;

	/// <summary>
	/// The speed of the device in meters per second.
	/// </summary>
	public double Speed => _hLocation?.HorizontalSpeed ?? _vLocation?.VerticalSpeed ?? 0;

	/// <summary>
	/// The original time of the location was captured.
	/// </summary>
	public DateTime StatusTime => _hLocation?.HorizontalStatusTime ?? _vLocation?.VerticalStatusTime ?? DateTime.MinValue;

	/// <inheritdoc />
	public object ValueObject => (object) _hLocation ?? _vLocation;

	#endregion

	#region Methods

	/// <inheritdoc />
	public void UpdateValues(ILocationProxy value)
	{
		switch (ProxyType)
		{
			case LocationProxyType.Horizontal
				when ValueObject is IHorizontalLocation currentValue
				&& value.ValueObject is IHorizontalLocation updateValue:
			{
				currentValue.UpdateWith(updateValue);
				break;
			}
			case LocationProxyType.Vertical
				when ValueObject is IVerticalLocation currentValue
				&& value.ValueObject is IVerticalLocation updateValue:
			{
				currentValue.UpdateWith(updateValue);
				break;
			}
		}
	}

	/// <inheritdoc />
	public void UpdateWith(ILocationProxy update, params string[] exclusions)
	{
		if (ProxyType != update.ProxyType)
		{
			return;
		}

		switch (ProxyType)
		{
			case LocationProxyType.Horizontal:
			{
				if (update.ValueObject is not IHorizontalLocation hUpdate)
				{
					return;
				}

				_hLocation.UpdateWith(hUpdate);
				break;
			}
			case LocationProxyType.Vertical:
			{
				if (update.ValueObject is not IVerticalLocation hUpdate)
				{
					return;
				}

				_vLocation.UpdateWith(hUpdate);
				break;
			}
		}
	}

	#endregion
}

/// <summary>
/// Represents a location proxy that generalizes accessors.
/// </summary>
public interface ILocationProxy : IUpdatable<ILocationProxy>
{
	#region Properties

	/// <summary>
	/// The accuracy of the horizontal location. (latitude, longitude)
	/// </summary>
	double Accuracy { get; }

	/// <summary>
	/// The reference system for accuracy.
	/// </summary>
	AccuracyReferenceType AccuracyReference { get; }

	/// <summary>
	/// Flags for the location of the provider.
	/// </summary>
	LocationFlags Flags { get; }

	/// <summary>
	/// Specifies if the Accuracy value is valid.
	/// </summary>
	bool HasAccuracy { get; }

	/// <summary>
	/// Specifies if the Heading value is valid
	/// </summary>
	bool HasHeading { get; }

	/// <summary>
	/// The location has a location.
	/// </summary>
	bool HasLocation { get; }

	/// <summary>
	/// Specifies if the Speed value is valid
	/// </summary>
	bool HasSpeed { get; }

	/// <summary>
	/// The heading of a device.
	/// </summary>
	double Heading { get; }

	/// <summary>
	/// The type of the location this proxy is for.
	/// </summary>
	LocationProxyType ProxyType { get; }

	/// <summary>
	/// The name of the source of the location. Ex. Wifi, GPS, Hardware, Simulated, etc
	/// </summary>
	string SourceName { get; }

	/// <summary>
	/// The speed of the device in meters per second.
	/// </summary>
	double Speed { get; }

	/// <summary>
	/// The original time of the location was captured.
	/// </summary>
	DateTime StatusTime { get; }

	/// <summary>
	/// The object this proxy is representing.
	/// </summary>
	object ValueObject { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Update the value of this proxy with the incoming proxy values.
	/// </summary>
	/// <param name="value"> </param>
	void UpdateValues(ILocationProxy value);

	#endregion
}