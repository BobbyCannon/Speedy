#region References

using Speedy.Plugins.Maui;
using static Android.Provider.Settings;
using Application = Android.App.Application;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui.Devices
{
    public class PlatformRuntimeInformation : MauiRuntimeInformation
	{
		#region Properties

		/// <inheritdoc />
		public override string DeviceId
		{
			get
			{
				var context = Application.Context;
				var id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
				return id;
			}
		}

		#endregion
	}
}