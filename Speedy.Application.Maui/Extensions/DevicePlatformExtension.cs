#region References

using DevicePlatform = Speedy.Devices.DevicePlatform;

#endregion

namespace Speedy.Application.Maui.Extensions
{
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
				{ Microsoft.Maui.Devices.DevicePlatform.Unknown, DevicePlatform.Unknown },
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

		public static DevicePlatform ToDevicePlatform(this Microsoft.Maui.Devices.DevicePlatform platform)
		{
			return _speedyLookup.ContainsKey(platform) ? _speedyLookup[platform] : DevicePlatform.Unknown;
		}

		public static Microsoft.Maui.Devices.DevicePlatform ToDevicePlatform(this DevicePlatform platform)
		{
			return _mauiLookup.ContainsKey(platform) ? _mauiLookup[platform] : Microsoft.Maui.Devices.DevicePlatform.Unknown;
		}

		#endregion
	}
}