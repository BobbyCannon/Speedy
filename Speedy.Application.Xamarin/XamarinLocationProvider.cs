#region References

using System;
using System.Threading;
using Speedy.Devices.Location;

#endregion

namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for LocationProvider.
/// </summary>
public class XamarinLocationProvider
{
	#region Fields

	private static readonly Lazy<LocationProvider> _implementation = new Lazy<LocationProvider>(CreateLocationProvider, LazyThreadSafetyMode.PublicationOnly);

	#endregion

	#region Properties

	/// <summary>
	/// Current plugin implementation to use
	/// </summary>
	public static LocationProvider Current
	{
		get
		{
			var ret = _implementation.Value;
			if (ret == null)
			{
				throw NotImplementedInReferenceAssembly();
			}
			return ret;
		}
	}

	/// <summary>
	/// Gets if the plugin is supported on the current platform.
	/// </summary>
	public static bool IsSupported => _implementation.Value != null;

	#endregion

	#region Methods

	private static LocationProvider CreateLocationProvider()
	{
		#if WINDOWS || ANDROID || IOS
			return new LocationProviderImplementation(null);
		#else
		return null;
		#endif
	}

	private static Exception NotImplementedInReferenceAssembly()
	{
		return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
	}

	#endregion
}