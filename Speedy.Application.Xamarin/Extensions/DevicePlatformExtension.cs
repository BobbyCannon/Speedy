#region References

using System.Collections.Generic;
using Speedy.Devices;
using XamarinEssentials = Xamarin.Essentials;

#endregion

namespace Speedy.Application.Xamarin.Extensions;

/// <summary>
/// Device Platform Extension
/// </summary>

public static class DevicePlatformExtension
{
	#region Fields

	private static readonly IReadOnlyDictionary<DevicePlatform, XamarinEssentials.DevicePlatform> _mauiLookup;
	private static readonly IReadOnlyDictionary<XamarinEssentials.DevicePlatform, DevicePlatform> _speedyLookup;

	#endregion

	#region Constructors

	static DevicePlatformExtension()
	{
		_speedyLookup = new Dictionary<XamarinEssentials.DevicePlatform, DevicePlatform>
		{
			{ XamarinEssentials.DevicePlatform.Android, DevicePlatform.Android },
			{ XamarinEssentials.DevicePlatform.iOS, DevicePlatform.IOS },
			{ XamarinEssentials.DevicePlatform.UWP, DevicePlatform.Windows },
			{ XamarinEssentials.DevicePlatform.Unknown, DevicePlatform.Unknown }
		};

		_mauiLookup = new Dictionary<DevicePlatform, XamarinEssentials.DevicePlatform>
		{
			{ DevicePlatform.Android, XamarinEssentials.DevicePlatform.Android },
			{ DevicePlatform.IOS, XamarinEssentials.DevicePlatform.iOS },
			{ DevicePlatform.Linux, XamarinEssentials.DevicePlatform.Unknown },
			{ DevicePlatform.Windows, XamarinEssentials.DevicePlatform.UWP },
			{ DevicePlatform.Unknown, XamarinEssentials.DevicePlatform.Unknown }
		};
	}

	#endregion

	#region Methods

	/// <summary>
	/// Converts a Xamarin Device Platform to a Speedy Device Platform.
	/// </summary>
	/// <param name="platform"> The Xamarin Device Platform. </param>
	/// <returns> The Speedy Device Platform. </returns>
	public static DevicePlatform ToDevicePlatform(this XamarinEssentials.DevicePlatform platform)
	{
		return _speedyLookup.ContainsKey(platform) ? _speedyLookup[platform] : DevicePlatform.Unknown;
	}

	/// <summary>
	/// Converts a Speedy Device Platform to a Xamarin Device Platform.
	/// </summary>
	/// <param name="platform"> The Speedy Device Platform. </param>
	/// <returns> The Xamarin Device Platform. </returns>
	public static XamarinEssentials.DevicePlatform ToDevicePlatform(this DevicePlatform platform)
	{
		return _mauiLookup.ContainsKey(platform) ? _mauiLookup[platform] : XamarinEssentials.DevicePlatform.Unknown;
	}

	#endregion
}