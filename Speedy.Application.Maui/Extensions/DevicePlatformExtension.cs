#region References

using DevicePlatform = Speedy.Data.DevicePlatform;

#endregion

namespace Speedy.Application.Maui.Extensions;

/// <summary>
/// Device Platform Extension
/// </summary>
public static class DevicePlatformExtension
{
	#region Fields

	private static readonly IReadOnlyDictionary<DevicePlatform, Microsoft.Maui.Devices.DevicePlatform> _mauiLookup;
	private static readonly IReadOnlyDictionary<Microsoft.Maui.Devices.DevicePlatform, DevicePlatform> _speedyLookup;

	#endregion

	#region Constructors

	static DevicePlatformExtension()
	{
		_speedyLookup = new Dictionary<Microsoft.Maui.Devices.DevicePlatform, DevicePlatform>
		{
			{ Microsoft.Maui.Devices.DevicePlatform.Android, DevicePlatform.Android },
			{ Microsoft.Maui.Devices.DevicePlatform.iOS, DevicePlatform.IOS },
			{ Microsoft.Maui.Devices.DevicePlatform.WinUI, DevicePlatform.Windows },
			{ Microsoft.Maui.Devices.DevicePlatform.Unknown, DevicePlatform.Unknown }
			// UWP is WinUI, cannot add duplicate key
			//{ Microsoft.Maui.Devices.DevicePlatform.UWP, DevicePlatform.Windows }
		};

		_mauiLookup = new Dictionary<DevicePlatform, Microsoft.Maui.Devices.DevicePlatform>
		{
			{ DevicePlatform.Android, Microsoft.Maui.Devices.DevicePlatform.Android },
			{ DevicePlatform.IOS, Microsoft.Maui.Devices.DevicePlatform.iOS },
			{ DevicePlatform.Linux, Microsoft.Maui.Devices.DevicePlatform.Unknown },
			{ DevicePlatform.Windows, Microsoft.Maui.Devices.DevicePlatform.WinUI },
			{ DevicePlatform.Unknown, Microsoft.Maui.Devices.DevicePlatform.Unknown }
		};
	}

	#endregion

	#region Methods

	/// <summary>
	/// Converts a Xamarin Device Platform to a Speedy Device Platform.
	/// </summary>
	/// <param name="platform"> The Xamarin Device Platform. </param>
	/// <returns> The Speedy Device Platform. </returns>
	public static DevicePlatform ToDevicePlatform(this Microsoft.Maui.Devices.DevicePlatform platform)
	{
		return _speedyLookup.ContainsKey(platform) ? _speedyLookup[platform] : DevicePlatform.Unknown;
	}

	/// <summary>
	/// Converts a Speedy Device Platform to a Xamarin Device Platform.
	/// </summary>
	/// <param name="platform"> The Speedy Device Platform. </param>
	/// <returns> The Xamarin Device Platform. </returns>
	public static Microsoft.Maui.Devices.DevicePlatform ToDevicePlatform(this DevicePlatform platform)
	{
		return _mauiLookup.ContainsKey(platform) ? _mauiLookup[platform] : Microsoft.Maui.Devices.DevicePlatform.Unknown;
	}

	#endregion
}