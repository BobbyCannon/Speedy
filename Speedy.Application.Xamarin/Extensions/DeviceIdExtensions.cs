#region References

#if ANDROID
using Android.Provider;
#elif IOS || __MACCATALYST__
using UIKit;
#elif WINDOWS
using System.Runtime.InteropServices.WindowsRuntime;
using Speedy.Application.Internal;
using Windows.System.Profile;
#endif

#endregion

namespace Speedy.Application.Xamarin.Extensions;

/// <summary>
/// Extension methods for <see cref="DeviceId" />.
/// </summary>
public static class DeviceIdExtensions
{
	#region Methods

	public static DeviceId AddVendorId(this DeviceId builder)
	{
		#if ANDROID
		var context = Android.App.Application.Context;
		var id = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
		builder.AddComponent("VendorId", new DeviceIdComponent(id));
		#elif IOS || __MACCATALYST__
		builder.AddComponent("VendorId", new DeviceIdComponent(UIDevice.CurrentDevice.IdentifierForVendor.AsString()));
		#elif WINDOWS
		var systemId = SystemIdentification.GetSystemIdForPublisher();
		if (systemId == null)
		{
			builder.AddComponent("VendorId", new DeviceIdComponent(string.Empty));
		}
		else
		{
			var systemIdBytes = systemId.Id.ToArray();
			var encoder = new Base32ByteArrayEncoder(Base32ByteArrayEncoder.CrockfordAlphabet);
			var id = encoder.Encode(systemIdBytes);
			builder.AddComponent("VendorId", new DeviceIdComponent(id));
		}
		#endif

		return builder;
	}


	#endregion
}