#region References

using System;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location (lat, long).
/// </summary>
public class LocationProxy
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
	}

	/// <summary>
	/// Create a proxy for a vertical location.
	/// </summary>
	public LocationProxy(IVerticalLocation location)
	{
		_vLocation = location;
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

	#endregion
}