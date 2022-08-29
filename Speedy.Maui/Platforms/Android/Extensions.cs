#region References

using System.Diagnostics.CodeAnalysis;
using Android.App;
using Android.Content;
using Android.OS;

#endregion

namespace Speedy.Maui.Platforms.Android
{
	public static class Extensions
	{
		#region Methods

		[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
		public static void StartForegroundServiceCompat<T>(this Context context, Bundle args = null) where T : Service
		{
			var intent = new Intent(context, typeof(T));
			if (args != null)
			{
				intent.PutExtras(args);
			}

			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				_ = context.StartForegroundService(intent);
			}
			else
			{
				context.StartService(intent);
			}
		}

		#endregion
	}
}