#region References

using System;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a full location from a LocationProvider. Contains horizontal and vertical location.
/// </summary>
public class Location : CloneableBindable<Location, ILocation<HorizontalLocation, VerticalLocation>>
	, ILocation<HorizontalLocation, VerticalLocation>
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
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public HorizontalLocation HorizontalLocation { get; set; }

	/// <inheritdoc />
	public string ProviderName { get; set; }

	/// <inheritdoc />
	public VerticalLocation VerticalLocation { get; set; }

	#endregion

	#region Methods

	
	/// <inheritdoc />
	public bool UpdateWith(ILocation update, params string[] exclusions)
	{
		var result = false;

		if (HorizontalLocation.ShouldUpdate(update))
		{
			result |= HorizontalLocation.UpdateWith(update, exclusions);
		}

		if (VerticalLocation.ShouldUpdate(update))
		{
			result |= VerticalLocation.UpdateWith(update, exclusions);
		}

		return result;
	}

	#endregion

	public override bool UpdateWith(Location update, params string[] exclusions)
	{
		throw new NotImplementedException();
	}

	public bool ShouldUpdate(ILocation<HorizontalLocation, VerticalLocation> update)
	{
		throw new NotImplementedException();
	}

	public bool UpdateWith(ILocation<HorizontalLocation, VerticalLocation> update, params string[] exclusions)
	{
		throw new NotImplementedException();
	}
}

/// <summary>
/// Represents a provider location.
/// </summary>
public interface ILocation : ILocation<IHorizontalLocation, IVerticalLocation>, IUpdatable<ILocation>
{
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
	/// The name of the provider that is the source of this location.
	/// </summary>
	string ProviderName { get; set; }

	/// <summary>
	/// The vertical location.
	/// </summary>
	TVerticalLocation VerticalLocation { get; set; }

	#endregion
}