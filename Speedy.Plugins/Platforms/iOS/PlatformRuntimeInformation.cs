#region References

using Speedy.Plugins.Maui;
using UIKit;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui.Devices
{
	public class PlatformRuntimeInformation : MauiRuntimeInformation
	{
		#region Properties

		public override string DeviceId => UIDevice.CurrentDevice.IdentifierForVendor.AsString();

		#endregion
	}
}