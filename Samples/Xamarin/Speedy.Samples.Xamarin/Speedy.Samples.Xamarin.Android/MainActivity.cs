#region References

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Speedy.Application.Xamarin;
using Speedy.Samples.Xamarin.Droid.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Platform = Xamarin.Essentials.Platform;

#endregion

[assembly: Dependency(typeof(FileService))]
namespace Speedy.Samples.Xamarin.Droid
{
	[Activity(Label = "Speedy.Samples.Xamarin", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class MainActivity : FormsAppCompatActivity
	{
		#region Methods

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			Platform.Init(this, savedInstanceState);
			Forms.Init(this, savedInstanceState);
			XamarinPlatform.Initialize(this);

			var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
			if (status != PermissionStatus.Granted)
			{
				await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			}
			
			status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
			if (status != PermissionStatus.Granted)
			{
				await Permissions.RequestAsync<Permissions.StorageWrite>();
			}
			
			LoadApplication(new App());
		}

		#endregion
	}
}