namespace Speedy.Devices.Location;

/// <summary>
/// Represents a full location from a LocationProvider. Contains horizontal and vertical location.
/// </summary>
public class Location : CloneableBindable<Location, ILocation<IHorizontalLocation, IVerticalLocation>>,
	ILocation<IHorizontalLocation, IVerticalLocation>
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
	public Location(IDispatcher dispatcher) : base(dispatcher)
	{
		HorizontalLocation = new HorizontalLocation();
		VerticalLocation = new VerticalLocation();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public IHorizontalLocation HorizontalLocation { get; set; }

	/// <inheritdoc />
	public IVerticalLocation VerticalLocation { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public bool ShouldUpdate(ILocation<IHorizontalLocation, IVerticalLocation> update)
	{
		return HorizontalLocation.ShouldUpdate(update)
			|| VerticalLocation.ShouldUpdate(update);
	}

	/// <inheritdoc />
	public override bool ShouldUpdate(object update)
	{
		return update switch
		{
			Location location => ShouldUpdate(location),
			ILocation<IHorizontalLocation, IVerticalLocation> location => ShouldUpdate(location),
			HorizontalLocation location => HorizontalLocation.ShouldUpdate(location),
			IHorizontalLocation location => HorizontalLocation.ShouldUpdate(location),
			VerticalLocation location => VerticalLocation.ShouldUpdate(location),
			IVerticalLocation location => VerticalLocation.ShouldUpdate(location),
			_ => base.ShouldUpdate(update)
		};
	}

	/// <inheritdoc />
	public bool UpdateWith(ILocation<IHorizontalLocation, IVerticalLocation> update, params string[] exclusions)
	{
		var result = false;

		result |= HorizontalLocation.UpdateWith(update.HorizontalLocation, exclusions);
		result |= VerticalLocation.UpdateWith(update.VerticalLocation, exclusions);

		return result;
	}

	/// <inheritdoc />
	public override bool UpdateWith(Location update, params string[] exclusions)
	{
		return UpdateWith(update, exclusions);
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			Location location => UpdateWith(location, exclusions),
			ILocation<IHorizontalLocation, IVerticalLocation> location => UpdateWith(location, exclusions),
			HorizontalLocation location => HorizontalLocation.UpdateWith(location, exclusions),
			IHorizontalLocation location => HorizontalLocation.UpdateWith(location, exclusions),
			VerticalLocation location => VerticalLocation.UpdateWith(location, exclusions),
			IVerticalLocation location => VerticalLocation.UpdateWith(location, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	#endregion
}

/// <summary>
/// Represents a provider location.
/// </summary>
public interface ILocation<THorizontalLocation, TVerticalLocation>
	: IBindable<ILocation<THorizontalLocation, TVerticalLocation>>
	where THorizontalLocation : class, IHorizontalLocation
	where TVerticalLocation : class, IVerticalLocation
{
	#region Properties

	/// <summary>
	/// The horizontal location.
	/// </summary>
	THorizontalLocation HorizontalLocation { get; set; }

	/// <summary>
	/// The vertical location.
	/// </summary>
	TVerticalLocation VerticalLocation { get; set; }

	#endregion
}