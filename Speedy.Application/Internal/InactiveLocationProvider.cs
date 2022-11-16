#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Devices.Location;

#endregion

namespace Speedy.Application.Internal;

/// <inheritdoc />
public class InactiveLocationProvider<T, THorizontalLocation, TVerticalLocation, T2>
	: LocationProvider<T, THorizontalLocation, TVerticalLocation, T2>
	where T : class, ILocation<THorizontalLocation, TVerticalLocation>, new()
	where THorizontalLocation : class, IHorizontalLocation, IUpdatable<THorizontalLocation>
	where TVerticalLocation : class, IVerticalLocation, IUpdatable<TVerticalLocation>
	where T2 : ILocationProviderSettings, new()
{
	#region Constructors

	/// <inheritdoc />
	public InactiveLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override string ProviderName => "Inactive Provider";

	#endregion

	#region Methods

	/// <inheritdoc />
	public override Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? token = null)
	{
		return Task.FromResult(CurrentValue);
	}

	/// <inheritdoc />
	public override Task StartMonitoringAsync()
	{
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public override Task StopMonitoringAsync()
	{
		return Task.CompletedTask;
	}

	#endregion
}