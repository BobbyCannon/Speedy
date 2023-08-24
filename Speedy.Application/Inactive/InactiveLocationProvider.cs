#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Data.Location;

#endregion

namespace Speedy.Application.Inactive;

/// <inheritdoc />
public abstract class InactiveLocationProvider<T, THorizontalLocation, TVerticalLocation, T2>
	: LocationProvider<T, THorizontalLocation, TVerticalLocation, T2>
	where T : class, ILocation<THorizontalLocation, TVerticalLocation>,  new()
	where THorizontalLocation : class, IHorizontalLocation 
	where TVerticalLocation : class, IVerticalLocation
	where T2 : ILocationProviderSettings, new()
{
	#region Constructors

	/// <inheritdoc />
	protected InactiveLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		HasPermission = true;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public sealed override bool HasPermission { get; protected set; }

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