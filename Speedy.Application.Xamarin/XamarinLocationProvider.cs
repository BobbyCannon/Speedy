﻿#region References

using Speedy.Devices.Location;
#if !(NETCOREAPP || WINDOWS_UWP || MONOANDROID || XAMARIN_IOS)
using Speedy.Application.Internal;
#endif

#endregion

namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for LocationProvider.
/// </summary>
public class XamarinLocationProvider<T, TH, TV, T2>
	#if NETCOREAPP || WINDOWS_UWP || MONOANDROID || XAMARIN_IOS
	: LocationProviderImplementation<T, TH, TV, T2>
	#else
	: InactiveLocationProvider<T, TH, TV, T2>
	#endif
	where T : class, ILocation<TH, TV>, ICloneable<T>, new()
	where TH : class, IHorizontalLocation, IUpdatable<TH>
	where TV : class, IVerticalLocation, IUpdatable<TV>
	where T2 : LocationProviderSettings, new()
{
	#region Constructors

	/// <inheritdoc />
	public XamarinLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}