﻿#region References

using Speedy.Data.Location;
#if !(WINDOWS || ANDROID || IOS)
using Speedy.Application.Internal;
#endif

#endregion

namespace Speedy.Application.Maui;

/// <summary>
/// Implementation for LocationProvider.
/// </summary>
public class MauiLocationProvider<T, TH, TV, T2>
	#if (WINDOWS || ANDROID || IOS)
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
	public MauiLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}