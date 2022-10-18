#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Devices.Location;

#endregion

namespace Speedy.Application.Internal;

public class InactiveLocationProvider<T, T2> : LocationProvider<T, T2>
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings, new()
{
	#region Constructors

	public InactiveLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	public override Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? token = null)
	{
		return Task.FromResult(LastReadLocation);
	}

	public override Task StartListeningAsync()
	{
		return Task.CompletedTask;
	}

	public override Task StopListeningAsync()
	{
		return Task.CompletedTask;
	}

	#endregion
}