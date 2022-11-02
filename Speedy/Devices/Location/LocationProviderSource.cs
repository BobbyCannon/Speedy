namespace Speedy.Devices.Location;

/// <summary>
/// The source of a location provider.
/// </summary>
public class LocationProviderSource : Bindable
{
	#region Constructors

	public LocationProviderSource() : this(null)
	{
	}

	public LocationProviderSource(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// The flag to determine if a source is enable or disabled.
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	/// The flag to determine if the source is currently being used to listen for location updates.
	/// </summary>
	public bool Listening { get; set; }

	/// <summary>
	/// The source for the location provider.
	/// </summary>
	public string Provider { get; set; }

	#endregion
}