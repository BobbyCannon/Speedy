#region References

using Speedy.Devices;

#if ANDROID || IOS || WINDOWS
using Speedy.Plugins.Maui;
using Speedy.Maui.Devices;
#endif

#endregion

namespace Speedy.Plugins.Devices
{
	public static class DeviceFactory
	{
		#region Fields

		#if ANDROID || IOS || WINDOWS
		private static IRuntimeInformation _runtimeInformation;
		private static SecureVault _secureVault;
		#endif

		#endregion

		#region Properties

		public static IRuntimeInformation RuntimeInformation
		{
			get
			{
				#if ANDROID || IOS || WINDOWS
				return _runtimeInformation ??= new PlatformRuntimeInformation();
				#else
				return null;
				#endif
			}
		}

		public static SecureVault SecureVault
		{
			get
			{
				#if ANDROID || IOS || WINDOWS
				return _secureVault ??= new MauiSecureVault();
				#else
				return null;
				#endif
			}
		}

		#endregion
	}
}