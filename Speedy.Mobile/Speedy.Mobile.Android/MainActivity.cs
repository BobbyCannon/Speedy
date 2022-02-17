#region References

using System.Net;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Platform = Xamarin.Essentials.Platform;

#endregion

namespace Speedy.Mobile.Droid
{
	[Activity(Label = "Speedy.Mobile", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	{
		#region Properties

		public MobileForegroundService ForegroundService { get; private set; }

		#endregion

		#region Methods

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		protected override void Dispose(bool disposing)
		{
			ForegroundService.StopService();
			base.Dispose(disposing);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(savedInstanceState);

			// Accept SSL issues for our domain?
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

			Platform.Init(this, savedInstanceState);
			Forms.Init(this, savedInstanceState);

			LoadApplication(new App());

			var permissionsProvider = new PermissionProvider(this);
			permissionsProvider.RequestPermission(PermissionType.All);

			CreateNotificationChannel();
			ForegroundService = new MobileForegroundService();
			ForegroundService.StartService();
		}

		private void CreateNotificationChannel()
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.O)
			{
				// Notification channels are new in API 26 (and not a part of the
				// support library). There is no need to create a notification 
				// channel on older versions of Android.
				return;
			}

			var name = Resources?.GetString(Resource.String.app_name);
			var description = GetString(Resource.String.app_description);
			var channel = new NotificationChannel(MobileForegroundService.ChannelId, name, NotificationImportance.Default)
			{
				Description = description
			};

			var notificationManager = (NotificationManager) GetSystemService(NotificationService);
			notificationManager?.CreateNotificationChannel(channel);
		}

		#endregion
	}
}