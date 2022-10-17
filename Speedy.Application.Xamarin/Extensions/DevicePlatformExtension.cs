#region References

using System.Collections.Generic;
using DevicePlatform = Speedy.Devices.DevicePlatform;
using XamarinEssentials = Xamarin.Essentials;

#endregion

namespace Speedy.Application.Xamarin.Extensions;

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
			{ XamarinEssentials.DevicePlatform.Unknown, DevicePlatform.Unknown },
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

	public static DevicePlatform ToDevicePlatform(this XamarinEssentials.DevicePlatform platform)
	{
		return _speedyLookup.ContainsKey(platform) ? _speedyLookup[platform] : DevicePlatform.Unknown;
	}

	public static XamarinEssentials.DevicePlatform ToDevicePlatform(this DevicePlatform platform)
	{
		return _mauiLookup.ContainsKey(platform) ? _mauiLookup[platform] : XamarinEssentials.DevicePlatform.Unknown;
	}

	#endregion
}