#region References

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Speedy.Serialization;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The manager for location.
/// </summary>
public class LocationManager : LocationManager<Location, IHorizontalLocation, IVerticalLocation>
{
}

/// <summary>
/// The manager for location.
/// </summary>
/// <typeparam name="TLocation"> The full location type. </typeparam>
/// <typeparam name="THorizontal"> The horizontal type. </typeparam>
/// <typeparam name="TVertical"> The vertical type. </typeparam>
public abstract class LocationManager<TLocation, THorizontal, TVertical>
	: Bindable, ILocationProvider<TLocation>
	where TLocation : class, ICloneable<TLocation>, THorizontal, TVertical, new()
	where THorizontal : class, IHorizontalLocation, ICloneable<THorizontal>
	where TVertical : class, IVerticalLocation, ICloneable<TVertical>
{
	#region Fields

	private readonly List<IHorizontalLocationProvider<THorizontal>> _horizontalProviders;
	private readonly List<IVerticalLocationProvider<TVertical>> _verticalProviders;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates a location manager.
	/// </summary>
	protected LocationManager()
	{
		_horizontalProviders = new List<IHorizontalLocationProvider<THorizontal>>();
		_verticalProviders = new List<IVerticalLocationProvider<TVertical>>();

		Comparer = new LocationComparer<TLocation, THorizontal, TVertical>();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The comparer for location updates.
	/// </summary>
	public LocationComparer<TLocation, THorizontal, TVertical> Comparer { get; }

	/// <summary>
	/// A read only list of horizontal location providers.
	/// </summary>
	public IReadOnlyList<IHorizontalLocationProvider<THorizontal>> HorizontalLocationProviders => _horizontalProviders.AsReadOnly();

	/// <summary>
	/// The manager is listening.
	/// </summary>
	public bool IsListening { get; private set; }

	/// <inheritdoc />
	public TLocation LastReadLocation => Comparer.Value;

	/// <summary>
	/// A read only list of vertical location providers.
	/// </summary>
	public IReadOnlyList<IVerticalLocationProvider<TVertical>> VerticalLocationProviders => _verticalProviders.AsReadOnly();

	#endregion

	#region Methods

	/// <summary>
	/// Add a vertical location provider.
	/// </summary>
	/// <param name="provider"> The provider to add. </param>
	public void AddProvider(IVerticalLocationProvider<TVertical> provider)
	{
		_verticalProviders.Add(provider);
	}

	/// <summary>
	/// Add a horizontal location provider.
	/// </summary>
	/// <param name="provider"> The provider to add. </param>
	public void AddProvider(IHorizontalLocationProvider<THorizontal> provider)
	{
		_horizontalProviders.Add(provider);
	}

	/// <inheritdoc />
	public async Task StartListeningAsync()
	{
		foreach (var provider in _horizontalProviders)
		{
			await provider.StartListeningAsync();
		}

		foreach (var provider in _verticalProviders)
		{
			await provider.StartListeningAsync();
		}

		IsListening = true;
	}

	/// <inheritdoc />
	public async Task StopListeningAsync()
	{
		foreach (var provider in _horizontalProviders)
		{
			await provider.StopListeningAsync();
		}

		foreach (var provider in _verticalProviders)
		{
			await provider.StopListeningAsync();
		}

		IsListening = false;
	}

	/// <summary>
	/// Triggers the <see cref="LocationChanged" /> event.
	/// </summary>
	/// <param name="e"> The location that changed. </param>
	protected virtual void OnLocationChanged(TLocation e)
	{
		LocationChanged?.Invoke(this, e);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<TLocation> LocationChanged;

	#endregion
}