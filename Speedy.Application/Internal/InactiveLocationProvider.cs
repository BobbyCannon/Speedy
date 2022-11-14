#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Devices.Location;
using Speedy.Serialization;

#endregion

namespace Speedy.Application.Internal;

/// <inheritdoc />
public class InactiveLocationProvider<T, T2> : LocationProvider<T, T2>
	where T : class, ILocation, ICloneable<T>, new()
	where T2 : LocationProviderSettings, new()
{
	#region Constructors

	/// <inheritdoc />
	public InactiveLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? token = null)
	{
		return Task.FromResult(LastReadLocation);
	}

	/// <inheritdoc />
	public override Task StartListeningAsync()
	{
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public override Task StopListeningAsync()
	{
		return Task.CompletedTask;
	}

	#endregion
}