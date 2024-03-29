﻿#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Extensions;

#endregion

namespace Speedy.Data.Location;

/// <summary>
/// Represents provider of location information for a device.
/// </summary>
public abstract class LocationInformationProvider<T>
	: InformationProvider<T>
	where T : class, ILocationInformation, new()
{
	#region Fields

	private readonly LocationInformationComparer<T> _comparer;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates the provider.
	/// </summary>
	protected LocationInformationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		_comparer = new LocationInformationComparer<T>(dispatcher);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the current location from the provider.
	/// </summary>
	/// <param name="timeout"> Timeout to wait. If null we use the default time from <see cref="LocationProviderSettings" />. </param>
	/// <param name="cancelToken"> An optional cancellation token. </param>
	/// <returns> The current location or null if not available. </returns>
	public T GetCurrentLocation(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		return GetCurrentLocationAsync(timeout, cancelToken).AwaitResults();
	}

	/// <summary>
	/// Gets the current location from the provider.
	/// </summary>
	/// <param name="timeout"> Timeout to wait. If null we use the default time from <see cref="LocationProviderSettings" />. </param>
	/// <param name="cancelToken"> An optional cancellation token. </param>
	/// <returns> The current location or null if not available. </returns>
	public abstract Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null);

	/// <inheritdoc />
	public override bool ShouldUpdate(T value, T update)
	{
		return _comparer.ShouldUpdate(value, update);
	}

	/// <inheritdoc />
	public override bool UpdateWith(ref T value, T update, params string[] exclusions)
	{
		return _comparer.UpdateWith(ref value, update, exclusions);
	}

	#endregion
}