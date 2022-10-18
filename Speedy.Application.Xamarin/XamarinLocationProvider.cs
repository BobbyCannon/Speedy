#region References

using System.ComponentModel;
using Speedy.Devices.Location;
using Speedy.Extensions;
#if !(NETCOREAPP || WINDOWS_UWP || MONOANDROID || XAMARIN_IOS)
using Speedy.Application.Internal;
#endif

#endregion

namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for LocationProvider.
/// </summary>
public class XamarinLocationProvider<T, T2>
	#if NETCOREAPP || WINDOWS_UWP || MONOANDROID || XAMARIN_IOS
	: LocationProviderImplementation<T, T2>
	#else
	: InactiveLocationProvider<T, T2>
	#endif
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings, new()
{
	#region Constructors

	public XamarinLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}