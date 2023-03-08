#region References

using Android.Content;
using Android.OS;
using Microsoft.Maui;

#endregion

namespace Speedy.Application.Maui.Platforms.Android;

public class MauiActivity : MauiAppCompatActivity
{
	#region Methods

	protected override void OnCreate(Bundle savedInstanceState)
	{
		MauiPlatform.Initialize(this);
		base.OnCreate(savedInstanceState);
	}

	protected override void OnResume()
	{
		base.OnResume();
		//MauiNfcDeviceImplementation.HandleResume();
	}

	protected override void OnNewIntent(Intent intent)
	{
		base.OnNewIntent(intent);
		//MauiNfcDeviceImplementation.HandleIntent(intent);
	}

	#endregion
}