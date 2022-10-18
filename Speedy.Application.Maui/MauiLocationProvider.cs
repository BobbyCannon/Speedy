#region References

using Speedy.Devices.Location;
#if !(WINDOWS || ANDROID || IOS)
using Speedy.Application.Internal;
#endif

#endregion

namespace Speedy.Application.Maui;

/// <summary>
/// Implementation for LocationProvider.
/// </summary>
public class MauiLocationProvider<T, T2>
	#if WINDOWS || ANDROID || IOS
	: LocationProviderImplementation<T, T2>
	#else
	: InactiveLocationProvider<T, T2>
	#endif
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings, new()
{
	public MauiLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}
}